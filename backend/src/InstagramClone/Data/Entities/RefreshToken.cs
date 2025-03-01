namespace InstagramClone.Data.Entities
{
	public class RefreshToken
	{
		public string UserID { get; set; }
		public virtual required User User { get; set; }
		public required string Token { get; set; }
		public DateTime ExpiresAt { get; set; }
	}
}
