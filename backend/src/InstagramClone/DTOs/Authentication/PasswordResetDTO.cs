using System.ComponentModel.DataAnnotations;

namespace InstagramClone.DTOs.Authentication
{
	public class PasswordResetDTO
	{
		[EmailAddress]
		public required string Email { get; set; }
		public required string Token { get; set; }
		public required string NewPassword { get; set; }
	}
}
