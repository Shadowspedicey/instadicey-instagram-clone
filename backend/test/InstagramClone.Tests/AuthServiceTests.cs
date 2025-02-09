using InstagramClone.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace InstagramClone.Tests
{
	public class AuthServiceTests
	{
		public class JwtTests
		{
			private readonly IConfiguration _configuration;
			public JwtTests()
			{
				var configuration = new ConfigurationBuilder()
					.AddUserSecrets<AuthServiceTests>()
					.Build();
				_configuration = configuration;
			}

			private async Task<bool> ValidateToken(string token)
			{
				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateIssuerSigningKey = false,
					IssuerSigningKeys = new List<SecurityKey>
					{
						new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:Schemes:Bearer:SigningKeys:0:Value"])),
						new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:Schemes:Bearer:SigningKeys:1:Value"]))
					}
				};
				var tokenHandler = new JwtSecurityTokenHandler();
				var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token, validationParameters);
				return tokenValidationResult.IsValid;
			}

			[Fact]
			public async Task GenerateToken_ReturnsValidToken()
			{
				AuthService authService = new AuthService(_configuration);
				string token = authService.GenerateToken();

				bool tokenIsValid = await ValidateToken(token);

				Assert.True(tokenIsValid);
			}
		}
	}
}
