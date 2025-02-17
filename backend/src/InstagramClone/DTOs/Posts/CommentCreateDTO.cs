using System.ComponentModel.DataAnnotations;

namespace InstagramClone.DTOs.Posts
{
	public class CommentCreateDTO
	{
		[StringLength(1000)]
		public required string Comment { get; set; }
	}
}
