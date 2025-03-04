namespace InstagramClone.Data.Entities
{
	public class ChatRoom
	{
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		public virtual ICollection<Message> Messages { get; set; } = [];
		public virtual required ICollection<User> Users { get; set; } = [];
		public Message? LastMessage => Messages.FirstOrDefault();
		public DateTime? LastUpdated => LastMessage?.CreatedAt;
	}
}
