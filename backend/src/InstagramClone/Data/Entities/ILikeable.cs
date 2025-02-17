namespace InstagramClone.Data.Entities
{
	public interface ILikeable
	{
		public void Like(User user);
		public void Unlike(User user);
		public ICollection<User> Likes { get; set; }
	}
}
