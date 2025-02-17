using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class LikeController(AppDbContext dbContext, ILikeService likeService) : ControllerBase
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly ILikeService _likeService = likeService;

		[HttpPost("post/{postID}")]
		public async Task<IActionResult> LikePost(string postID)
		{
			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return this.ProblemWithErrors(statusCode: 404, detail: "Post was not found.", errors: new[] { new CodedError(ErrorCode.NotFound, "Post was not found.") });

			var result = await _likeService.Like(User, post);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("post/remove/{postID}")]
		public async Task<IActionResult> UnlikePost(string postID)
		{
			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return this.ProblemWithErrors(statusCode: 404, detail: "Post was not found.", errors: new[] { new CodedError(ErrorCode.NotFound, "Post was not found.") });

			var result = await _likeService.Unlike(User, post);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("comment/{commentID}")]
		public async Task<IActionResult> LikeComment(string commentID)
		{
			Comment? comment = await _dbContext.Comments.FindAsync(commentID);
			if (comment is null)
				return this.ProblemWithErrors(statusCode: 404, detail: "Comment was not found.", errors: new[] { new CodedError(ErrorCode.NotFound, "Comment was not found.") });

			var result = await _likeService.Like(User, comment);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("comment/remove/{commentID}")]
		public async Task<IActionResult> UnlikeComment(string commentID)
		{
			Comment? comment = await _dbContext.Comments.FindAsync(commentID);
			if (comment is null)
				return this.ProblemWithErrors(statusCode: 404, detail: "Comment was not found.", errors: new[] { new CodedError(ErrorCode.NotFound, "Comment was not found.") });

			var result = await _likeService.Unlike(User, comment);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}
	}
}
