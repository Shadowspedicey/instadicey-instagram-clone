namespace InstagramClone.Data.Entities
{
	public class Message
	{
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		public required string Content { get; set; }
		public virtual required User User { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
