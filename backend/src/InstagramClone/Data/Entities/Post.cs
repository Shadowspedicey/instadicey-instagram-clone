using InstagramClone.DTOs.Posts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
				User = new PostGetDTO.PostOwnerDTO
				{
					Username = User.UserName!,
					ProfilePic = User.ProfilePic!
				}
			};
		}
	}
}
