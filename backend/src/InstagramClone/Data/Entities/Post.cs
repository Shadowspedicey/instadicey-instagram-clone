using InstagramClone.DTOs.Posts;
using InstagramClone.DTOs.Profile;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace InstagramClone.Data.Entities
{
	public class Post
	{
		[Key]
		[StringLength(26)]
		public string ID { get; set; } = Ulid.NewUlid().ToString();
		[StringLength(1000)]
		public string? Caption { get; set; }
		public required string Photo { get; set; }
		public virtual required User User { get; set; }
		public virtual ICollection<Comment> Comments { get; set; } = [];
		public virtual ICollection<User> Likes { get; set; } = [];
		public required DateTime CreatedAt { get; set; }

		public PostGetDTO GetDTO(string fileDownloadEndpoint)
		{
			return new PostGetDTO
			{
				ID = ID,
				Caption = Caption,
				Photo = $"{fileDownloadEndpoint}{Photo}",
				CreatedAt = CreatedAt,
				User = new UserMinimalProfileDTO(User.UserName!, User.ProfilePic!, fileDownloadEndpoint)
			};
		}
		public UserProfilePost GetMinimalDTO(string fileDownloadEndpoint) => new()
		{
			ID = ID,
			Photo = $"{fileDownloadEndpoint}{Photo}",
			LikesCount = Likes.Count,
			CommentsCount = Comments.Count
		};
	}
}
