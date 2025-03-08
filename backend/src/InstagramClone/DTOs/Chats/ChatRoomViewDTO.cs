using InstagramClone.DTOs.Profile;

namespace InstagramClone.DTOs.Chats
{
	public class ChatRoomViewDTO
	{
		public required string ID { get; set; }
		public required ICollection<UserMinimalProfileDTO> Users { get; set; }
		public required MessageViewDTO? LastMessage { get; set; }
		public required DateTime? LastUpdated { get; set; }
	}
}
