using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstagramClone.Data.Entities
{
	[Table("Comments")]
	public class Comment
	{
		public int ID { get; set; }
		[StringLength(1000)]
		public required string Content { get; set; }
		public virtual required User User { get; set; }
		public virtual required Post Post { get; set; }
		public virtual ICollection<User> Likes { get; set; } = [];
		public required DateTime CreatedAt { get; set; }
	}
}
