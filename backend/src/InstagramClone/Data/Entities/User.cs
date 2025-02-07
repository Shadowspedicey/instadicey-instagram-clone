using InstagramClone.Data.Annotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstagramClone.Data.Entities
{
	public class User : IdentityUser
	{
		[StringLength(20, MinimumLength = 1)]
		[EnglishOnly]
		public required string Username { get; set; }
		[StringLength(50)]
		public required string RealName { get; set; }
		[Url]
		public string? ProfilePic { get; set; }
		public required bool IsVerified { get; set; }
		public virtual required ICollection<User> RecentSearches { get; set; } = [];
		public required DateTime CreatedAt { get; set; }
		public required DateTime LastLogin { get; set; }

		public virtual required ICollection<User> Following { get; set; } = [];
		public virtual required ICollection<User> Followers { get; set; } = [];

		public virtual ICollection<Post> Posts { get; set; } = [];
		public virtual ICollection<Post> LikedPosts { get; set; } = [];
		public virtual ICollection<Post> SavedPosts { get; set; } = [];
	}
}
