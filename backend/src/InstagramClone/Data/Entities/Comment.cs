using InstagramClone.DTOs.Posts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstagramClone.Data.Entities
{
	[Table("Comments")]
	public class Comment : ILikeable
	{
		[Key]
		[StringLength(26)]
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		[StringLength(1000)]
		public required string Content { get; set; }
		public virtual required User User { get; set; }
		public virtual required Post Post { get; set; }
		public virtual ICollection<User> Likes { get; set; } = [];
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public void Like(User user) => Likes.Add(user);
		public void Unlike(User user) => Likes.Remove(user);

		public CommentViewDTO ToDTO(string fileDownloadEndpoint) => new()
		{
			ID = ID,
			Comment = Content,
			User = User.ToMinimalDTO(fileDownloadEndpoint),
			PostID = Post.ID,
			Likes = Likes.Select(u => u.ToMinimalDTO(fileDownloadEndpoint)).ToList(),
			CreatedAt = CreatedAt,
		};
	}
}
