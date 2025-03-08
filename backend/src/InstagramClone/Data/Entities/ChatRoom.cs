using InstagramClone.DTOs.Chats;

namespace InstagramClone.Data.Entities
{
	public class ChatRoom
	{
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		public virtual ICollection<Message> Messages { get; set; } = [];
		public virtual required ICollection<User> Users { get; set; } = [];
		public IOrderedEnumerable<Message> SortedMessages => Messages.OrderBy(x => x.CreatedAt);
		public Message? LastMessage => SortedMessages.LastOrDefault();
		public DateTime? LastUpdated => LastMessage?.CreatedAt;

		public ChatRoomViewDTO ToViewDTO(string fileDownloadEndpoint) => new()
		{
			ID = ID,
			Users = Users.Select(u => u.ToMinimalDTO(fileDownloadEndpoint)).ToList(),
			LastMessage = LastMessage?.ToViewDTO(fileDownloadEndpoint),
			LastUpdated = LastUpdated
		};
	}
}
