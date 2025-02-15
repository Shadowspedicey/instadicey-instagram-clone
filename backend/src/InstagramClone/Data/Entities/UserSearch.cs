namespace InstagramClone.Data.Entities
{
	public class UserSearch
	{
		public string UserID { get; set; }
		public virtual required User User { get; set; }
		public string SearchedUserID { get; set; }
		public virtual required User SearchedUser { get; set; }
		public DateTime SearchedAt { get; set; }
	}
}
