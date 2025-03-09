using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class PostController(IPostsService postsService) : ControllerBase
	{
		private readonly IPostsService _postsService = postsService;
		private string DownloadFileEndpoint => $"{Request.Scheme}://{Request.Host}/file/";

		[Authorize]
		[HttpPost("create")]
		public async Task<IActionResult> CreatePost(PostCreateDTO postDTO)
		{
			var result = await _postsService.CreatePost(User, postDTO, HttpContext.RequestAborted);
			if (result.IsSuccess)
				return CreatedAtAction(nameof(GetPost), new { postID = result.Value.ID }, result.Value.ToDTO(DownloadFileEndpoint));
			else
				return BadRequest();
		}

		[HttpGet("{postID}")]
		public async Task<IActionResult> GetPost(string postID)
		{
			var result = await _postsService.GetPost(postID);
			if (result.IsSuccess)
				return Ok(result.Value.ToDTO(DownloadFileEndpoint));
			else if (result.HasCodedErrorWithCode(ErrorCode.NotFound))
				return this.Problem(statusCode: 404, detail: "Post was not found.");
			else
				return this.ProblemWithErrors(statusCode: 400, errors: result.Errors.Select(e => e.Metadata));
		}

		[HttpGet("{postID}/more")]
		public async Task<IActionResult> GetMorePosts(string postID)
		{
			var result = await _postsService.GetMorePosts(postID);

			return result.IsSuccess ? Ok(result.Value.Select(p => p.ToMinimalDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}

		[Authorize]
		[HttpPost("delete/{postID}")]
		public async Task<IActionResult> DeletePost(string postID)
		{
			var result = await _postsService.DeletePost(User, postID);

			if (result.IsSuccess)
				return NoContent();
			else if (result.HasCodedErrorWithCode(ErrorCode.InsufficientPermissions))
				return Problem(statusCode: 403, detail: result.Errors[0].Message);
			else if (result.HasCodedErrorWithCode(ErrorCode.NotFound))
				return Problem(statusCode: 404, detail: result.Errors[0].Message);
			else
				return this.ProblemWithErrors(statusCode: 400, detail: result.Errors[0].Message, errors: result.Errors.Select(e => e.Metadata));
		}
	}
}
