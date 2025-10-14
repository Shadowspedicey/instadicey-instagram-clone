using InstagramClone.Data.Entities;

namespace InstagramClone.DTOs.Authentication
{
	public class LoginSuccessDTO
	{
		public required string Token { get; set; }
		public required string RefreshToken { get; set; }
	}
}
