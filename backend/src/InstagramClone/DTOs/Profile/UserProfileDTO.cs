using InstagramClone.DTOs.Posts;

namespace InstagramClone.DTOs.Profile
{
	public class UserProfileDTO
	{
		public required string Username { get; set; }
		public string? RealName { get; set; }
		public string? ProfilePic { get; set; }
		public string? Bio { get; set; }
		public required bool IsVerified { get; set; }
		public virtual required ICollection<UserMinimalProfileDTO> Following { get; set; } = [];
		public virtual required ICollection<UserMinimalProfileDTO> Followers { get; set; } = [];

		public virtual ICollection<PostMinimalViewDTO> Posts { get; set; } = [];
	}
}
