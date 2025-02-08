using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class AuthService(IConfiguration configuration, UserManager<User>? userManager = null)
	{
		private readonly IConfiguration _configuration = configuration;
		private readonly UserManager<User> _userManager = userManager;

		public async Task<(IdentityResult, User?)> RegisterUser(UserRegisterDTO userData)
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
				return (result, null);
			return (result, newUser);
		}

		public string GenereateToken(User? user = null)
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
