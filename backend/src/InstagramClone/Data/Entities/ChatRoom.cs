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
	}
}
