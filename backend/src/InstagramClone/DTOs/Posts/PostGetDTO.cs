namespace InstagramClone.DTOs.Posts
{
	public class PostGetDTO
	{
		public required string ID { get; set; }
		public string? Caption { get; set; }
		public required string Photo { get; set; }
		public required DateTime CreatedAt { get; set; }
		public required PostOwnerDTO User { get; set; }
		public class PostOwnerDTO
		{
			public required string Username { get; set; }
			public required string ProfilePic { get; set; }
		}
	}
}
