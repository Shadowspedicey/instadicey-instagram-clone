using InstagramClone.DTOs.Profile;

namespace InstagramClone.DTOs.Posts
{
	public class CommentViewDTO
	{
		public required string ID { get; set; }
		public required string Comment { get; set; }
		public required string PostID { get; set; }
		public required UserMinimalProfileDTO User { get; set; }
		public required ICollection<UserMinimalProfileDTO> Likes { get; set; }
		public required DateTime CreatedAt { get; set; }
	}
}
