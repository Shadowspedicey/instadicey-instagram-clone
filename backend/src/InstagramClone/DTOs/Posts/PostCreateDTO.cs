using InstagramClone.Data.Annotations;

namespace InstagramClone.DTOs.Posts
{
	public class PostCreateDTO
	{
		public string? Caption { get; set; }
		[ImageOnly]
		[MaxFileSize(8)]
		public required IFormFile Photo { get; set; }
	}
}
