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

		[Authorize]
		[HttpPost("create")]
		public async Task<IActionResult> CreatePost(PostCreateDTO postDTO)
		{
			var result = await _postsService.CreatePost(User, postDTO);
			if (result.IsSuccess)
				return CreatedAtAction(nameof(GetPost), new { postID = result.Value.ID }, result.Value);
			else
				return BadRequest();
		}

		[HttpGet("{postID}")]
		public async Task<IActionResult> GetPost(string postID)
		{
			var result = await _postsService.GetPost(postID);
			if (result.IsSuccess)
				return Ok(result.Value);
			else if (result.HasError(e => e.Message == "Not found."))
				return NotFound();
			else
				return Problem(statusCode: 400, detail: result.Errors.FirstOrDefault()?.Message);
		}
	}
}
