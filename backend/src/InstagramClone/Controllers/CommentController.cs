using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[Authorize]
	[ApiController]
	public class CommentController(AppDbContext dbContext, ICommentService commentService) : ControllerBase
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly ICommentService _commentService = commentService;

		[HttpPost("post/{postID}")]
		public async Task<IActionResult> AddPostComment(string postID, CommentCreateDTO commentDTO)
		{
			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return this.ProblemWithErrors(statusCode: 404, detail: "Post was not found.", errors: new[] { new CodedError(ErrorCode.NotFound, "Post was not found.") });

			var result = await _commentService.AddComment(User, post, commentDTO.Comment);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("delete/{commentID}")]
		public async Task<IActionResult> DeleteComment(string commentID)
		{
			var result = await _commentService.DeleteComment(User, commentID);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}
	}
}
