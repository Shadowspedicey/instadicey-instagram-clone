using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Authentication;
using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class AuthService(
		IConfiguration configuration,
		UserManager<User>? userManager = null,
		IEmailSender? emailSender = null)
	{
		private readonly IConfiguration _configuration = configuration;
		private readonly UserManager<User> _userManager = userManager;
		private readonly IEmailSender _emailSender = emailSender;

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
				return IdentityResult.Failed(new IdentityError() { Code = "InvalidEmail" });

			if (user is null && encodedEmail is not null)
				user = await _userManager.FindByEmailAsync(WebUtility.UrlDecode(encodedEmail));
			if (user is null)
				return IdentityResult.Failed(new IdentityError() { Code = "UserNotFound" });

			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var encodedToken = WebUtility.UrlEncode(token);

			await _emailSender.SendAccountVerificationEmail(user, encodedToken);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> ConfirmEmail(string encodedEmail, string encodedToken)
		{
			string email = WebUtility.UrlDecode(encodedEmail);

			User? user = await _userManager.FindByEmailAsync(email);
			if (user is null)
				return IdentityResult.Failed(new IdentityError() { Code = "UserNotFound" });

			var result = await _userManager.ConfirmEmailAsync(user, encodedToken);
			return result;
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
						new("isVerified", user.IsVerified.ToString(), ClaimValueTypes.Boolean)
					);

			var secToken = new JwtSecurityToken
			(
				signingCredentials: signingCredentials,
				claims: claims,
				expires: DateTime.Now.AddDays(1)
			);

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(secToken);
		}
	}
}
