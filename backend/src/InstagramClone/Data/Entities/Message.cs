using InstagramClone.DTOs.Chats;

namespace InstagramClone.Data.Entities
{
	public class Message
	{
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		public required string Content { get; set; }
		public virtual required User User { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public MessageViewDTO ToViewDTO(string fileDownloadEndpoint) => new()
		{
			ID = ID,
			Message = Content,
			User = User.ToMinimalDTO(fileDownloadEndpoint),
			CreatedAt = CreatedAt
		};
	}
}
