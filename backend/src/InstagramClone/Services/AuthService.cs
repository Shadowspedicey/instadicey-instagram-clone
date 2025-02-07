using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class AuthService(IConfiguration configuration)
	{
		private readonly IConfiguration _configuration = configuration;

		public string GenereateToken()
		{
			var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:Schemes:Bearer:SigningKeys:1:Value"]));
			var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var secToken = new JwtSecurityToken
			(
				signingCredentials: signingCredentials,
				claims:
				[
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
				],
				expires: DateTime.Now.AddDays(1)
			);

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.WriteToken(secToken);
		}
	}
}
