using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Authentication;
using InstagramClone.Hubs;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace InstagramClone.Services
{
	public class AuthService(
		IConfiguration configuration,
		UserManager<User>? userManager = null,
		AppDbContext? dbContext = null,
		IEmailSender? emailSender = null,
		IHubContext<AuthHub>? authHub = null,
		UserConnectionManager? connections = null)
	{
		private readonly IConfiguration _configuration = configuration;
		private readonly UserManager<User> _userManager = userManager;
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IEmailSender _emailSender = emailSender;
		private readonly IHubContext<AuthHub> _authHub = authHub;
		private readonly UserConnectionManager _connections = connections;

		public async Task<IdentityResult> RegisterUser(UserRegisterDTO userData)
		{
			User newUser = new()
			{
				Email = userData.Email,
				UserName = userData.Username,
				RealName = userData.RealName,
				IsVerified = false,
				CreatedAt = DateTime.Now,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				Following = [],
				Followers = []
			};

			var result = await _userManager.CreateAsync(newUser, userData.Password);
			if (!result.Succeeded)
				return result;

			await SendAccountVerificationEmail(user: newUser);

			return result;
		}

		public async Task<(IdentityResult, User?)> CheckLoginInfo(UserLoginDTO userLoginData)
		{
			string email = userLoginData.Email;
			string password = userLoginData.Password;

			User? user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return (IdentityResult.Failed(new IdentityError() { Code = "InvalidCredentials", Description = "Email or password are invalid." }), null);

			bool loginResult = await _userManager.CheckPasswordAsync(user, password);
			if (!loginResult)
				return (IdentityResult.Failed(new IdentityError() { Code = "InvalidCredentials", Description = "Email or password are invalid." }), null);

			bool emailConfirmationCheckResult = await _userManager.IsEmailConfirmedAsync(user);
			if (!emailConfirmationCheckResult)
				return (IdentityResult.Failed(new IdentityError() { Code = "EmailNotVerified", Description = "Email address is not verified." }), null);

			return (IdentityResult.Success, user);
		}

		public async Task<IdentityResult> SendAccountVerificationEmail(User? user = null, string? encodedEmail = null)
		{
			if (user is null & string.IsNullOrEmpty(encodedEmail))
				return IdentityResult.Failed(new IdentityError() { Code = "InvalidEmail", Description = "Email is invalid." });

			if (user is null && encodedEmail is not null)
				user = await _userManager.FindByEmailAsync(WebUtility.UrlDecode(encodedEmail));
			if (user is null)
				return IdentityResult.Failed(new IdentityError() { Code = "UserNotFound", Description = "A user with an associated email was not found." });

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			await _emailSender.SendAccountVerificationEmail(user.Email!, token);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> ConfirmEmail(string email, string token)
		{
			User? user = await _userManager.FindByEmailAsync(email);
			if (user is null)
				return IdentityResult.Failed(new IdentityError() { Code = "UserNotFound" });

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				string? connectionID = _connections.GetConnection(email);
				if (connectionID is not null)
					await _authHub.Clients.Client(connectionID).SendAsync("VerifyEmail");
			}
			return result;
		}

		public async Task<IdentityResult> SendEmailChangeRequest(ClaimsPrincipal userClaimsPrinicipal, string newEmail, string password)
		{
			User user = (await _userManager.GetUserAsync(userClaimsPrinicipal))!;

			bool passwordIsValid = await _userManager.CheckPasswordAsync(user, password);
			if (!passwordIsValid)
				return IdentityResult.Failed(new IdentityError() { Code = Enum.GetName(ErrorCode.InvalidCredentials)!, Description = "The password is incorrect."});

			var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

			await _emailSender.SendAccountVerificationEmail(newEmail, token);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> ConfirmEmailChange(ClaimsPrincipal userClaimsPrinciple, string newEmail, string token)
		{
			User user = (await _userManager.GetUserAsync(userClaimsPrinciple))!;
			var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

			return result;
		}

		public async Task<IdentityResult> ChangePassword(ClaimsPrincipal userClaimsPrinciple, string currentPassword, string newPassword)
		{
			User user = (await _userManager.GetUserAsync(userClaimsPrinciple))!;
			return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
		}

		public async Task<(IdentityResult, User?)> RefreshToken(string token)
		{
			var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
			if (refreshToken is null)
				return (IdentityResult.Failed(new IdentityError() { Code = "NotFound", Description = "Refresh token doesn't exist." }), null);

			if (refreshToken.ExpiresAt < DateTime.UtcNow)
				return (IdentityResult.Failed(new IdentityError() { Code = "TokenExpired", Description = "Refresh token expired. Please login again." }), null);

			return (IdentityResult.Success, refreshToken.User);
		}

		public string GenerateToken(string downloadFileEndpoint = "", User? user = null)
		{
			var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:Schemes:Bearer:SigningKeys:1:Value"]));
			var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>()
			{
					new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
			};
			if (user is not null)
				claims.AddRange
					(
						new("sub", user.Id),
						new("email", user.Email, ClaimValueTypes.Email),
						new("profilePic", $"{downloadFileEndpoint}{user.ProfilePic}", ClaimTypes.Uri),
						new("username", user.UserName),
						new("isVerified", user.IsVerified.ToString(), ClaimValueTypes.Boolean),
						new("realName", user.RealName ?? ""),
						new("bio", user.Bio ?? "")
					);

			var secToken = new JwtSecurityToken
			(
				signingCredentials: signingCredentials,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(5)
			);

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(secToken);
		}

		public async Task<RefreshToken> GenerateRefreshToken(User user)
		{
			string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

			var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.User.Id == user.Id);

			if (refreshToken is null)
			{
				refreshToken = new() { Token = token, User = user };
				_dbContext.Add(refreshToken);
			}
			else
				refreshToken.Token = token;
			refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(3);

			await _dbContext.SaveChangesAsync();
			return refreshToken;
		}
	}
}
