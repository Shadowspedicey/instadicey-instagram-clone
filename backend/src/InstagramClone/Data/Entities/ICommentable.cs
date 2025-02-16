using FluentResults;

namespace InstagramClone.Data.Entities
{
	public interface ICommentable
	{
		public void AddComment(Comment comment);
		public ICollection<Comment> SortedComments { get; }
	}
}
