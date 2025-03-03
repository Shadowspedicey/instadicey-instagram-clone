namespace InstagramClone.DTOs.Authentication
{
	public class PasswordChangeDTO
	{
		public required string CurrentPassword { get; set; }
		public required string NewPassword { get; set; }
	}
}
