using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstagramClone.Data.Entities
{
	public class Post
	{
		[Key]
		public required int ID { get; set; }
		[StringLength(1000)]
		public string? Caption { get; set; }
		[Url]
		public required string Photo { get; set; }
		public virtual required User User { get; set; }
		public virtual ICollection<Comment> Comments { get; set; } = [];
		public virtual ICollection<User> Likes { get; set; } = [];
		public required DateTime CreatedAt { get; set; }
	}
}
