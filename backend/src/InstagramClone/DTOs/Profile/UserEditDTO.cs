using InstagramClone.Data.Annotations;
using System.ComponentModel.DataAnnotations;

namespace InstagramClone.DTOs.Profile
{
	public class UserEditDTO
	{
		[Username]
		public required string Username { get; set; }
		[StringLength(50)]
		public string? RealName { get; set; }
		[MaxFileSize(10)]
		[ImageOnly]
		public IFormFile? NewProfilePic { get; set; }
		[MaxLength(150)]
		public string? Bio { get; set; }
	}
}
