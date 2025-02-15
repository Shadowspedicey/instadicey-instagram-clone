namespace InstagramClone.DTOs.Profile
{
	public class UserProfilePost
	{
		public required string ID { get; set; }
		public required string Photo { get; set; }
		public int LikesCount { get; set; } = 0;
		public int CommentsCount { get; set; } = 0;
	}
}
