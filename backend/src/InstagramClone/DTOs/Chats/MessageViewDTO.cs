using InstagramClone.DTOs.Profile;

namespace InstagramClone.DTOs.Chats
{
	public class MessageViewDTO
	{
		public required string ID { get; set; }
		public required string Message { get; set; }
		public required UserMinimalProfileDTO User { get; set; }
		public required DateTime CreatedAt { get; set; }
	}
}
