namespace InstagramClone.Data.Entities
{
	public interface ICommentable
	{
		public void AddComment(Comment comment);
		public IOrderedEnumerable<Comment> SortedComments { get; }
	}
}
