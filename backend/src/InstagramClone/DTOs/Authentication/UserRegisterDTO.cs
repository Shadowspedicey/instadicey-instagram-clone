using InstagramClone.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace InstagramClone.DTOs.Authentication
{
	public class UserRegisterDTO
	{
		[EmailAddress]
		public required string Email { get; set; }
		[StringLength(50)]
		public required string RealName { get; set; }
		[Username]
		public required string Username { get; set; }
		public required string Password { get; set; }
	}
}
