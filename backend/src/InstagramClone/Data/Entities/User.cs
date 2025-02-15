using InstagramClone.DTOs.Profile;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InstagramClone.Data.Entities
{
	public class User : IdentityUser
	{
		[StringLength(50)]
		public string? RealName { get; set; }
		public string? ProfilePic { get; set; }
		[StringLength(150)]
		public string? Bio { get; set; }
		public required bool IsVerified { get; set; }
		public virtual required ICollection<User> RecentSearches { get; set; } = [];
		public required DateTime CreatedAt { get; set; }
		public required DateTime LastLogin { get; set; }

		public virtual required ICollection<User> Following { get; set; } = [];
		public virtual required ICollection<User> Followers { get; set; } = [];

		public virtual ICollection<Post> Posts { get; set; } = [];
		public virtual ICollection<Post> LikedPosts { get; set; } = [];
		public virtual ICollection<Post> SavedPosts { get; set; } = [];

		public UserProfileDTO GetDTO(string downloadEndpoint) => new()
		{
			Username = UserName!,
			RealName = RealName,
			Bio = Bio,
			ProfilePic = $"{downloadEndpoint}{ProfilePic}",
			IsVerified = IsVerified,
			Following = Following.Select(u => u.GetMinimalDTO(downloadEndpoint)).ToList(),
			Followers = Followers.Select(u => u.GetMinimalDTO(downloadEndpoint)).ToList(),
			Posts = Posts.Select(p => p.GetMinimalDTO(downloadEndpoint)).ToList(),
		};

		public UserMinimalProfileDTO GetMinimalDTO(string downloadEndpoint) => new(UserName!, ProfilePic!, downloadEndpoint);
	}
}
