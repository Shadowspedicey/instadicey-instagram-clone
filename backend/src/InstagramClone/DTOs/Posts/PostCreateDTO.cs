namespace InstagramClone.DTOs.Posts
{
	public class PostCreateDTO
	{
		public string? Caption { get; set; }
		public required IFormFile Photo { get; set; }
	}
}
