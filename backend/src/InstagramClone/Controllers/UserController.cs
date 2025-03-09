using InstagramClone.DTOs.Profile;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InstagramClone.Filters;
using InstagramClone.Data.Annotations;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[Authorize]
	[ApiController]
	public class UserController(IUserService userService) : ControllerBase
	{
		private readonly IUserService _userService = userService;
		private string DownloadFileEndpoint => $"{Request.Scheme}://{Request.Host}/file/";

		[AllowAnonymous]
		[HttpGet("{username}")]
		public async Task<IActionResult> Get(string username)
		{
			var result = await _userService.GetUser(username);

			return result.IsSuccess ? Ok(result.Value.ToDTO(DownloadFileEndpoint)) : this.AppropriateResponseBasedOnResult(result);
		}

		[AllowAnonymous]
		[HttpGet("search")]
		public async Task<IActionResult> Search(string username)
		{
			var result = await _userService.SearchForUsers(username);

			return result.IsSuccess ? Ok(result.Value.Select(u => u.ToMinimalDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}

		[EditDTOUsernameLowercaseFilter]
		[HttpPost("edit")]
		public async Task<IActionResult> Edit(UserEditDTO userData)
		{
			var result = await _userService.EditUserData(User, userData);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("feed")]
		public async Task<IActionResult> GetFeed()
		{
			var result = await _userService.GetUserFeed(User);

			return result.IsSuccess ? Ok(result.Value.Select(p => p.ToDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("edit/profile-pic")]
		public async Task<IActionResult> UpdateProfilePic([MaxFileSize(10)] [ImageOnly] IFormFile newProfilePic, CancellationToken cancellationToken)
		{
			var result = await _userService.ChangeProfilePic(User, newProfilePic, cancellationToken);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("edit/profile-pic/reset")]
		public async Task<IActionResult> ResetProfilePic()
		{
			var result = await _userService.ResetProfilePic(User);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("follow/{username}")]
		public async Task<IActionResult> Follow(string username)
		{
			var result = await _userService.FollowUser(User, username);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("unfollow/{username}")]
		public async Task<IActionResult> Unfollow(string username)
		{
			var result = await _userService.UnfollowUser(User, username);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("following-check/{username}")]
		public async Task<IActionResult> FollowingCheck(string username)
		{
			var result = await _userService.FollowingCheck(User, username);

			return result.IsSuccess ? Ok(result.Value) : this.AppropriateResponseBasedOnResult(result);
		}



		[HttpPost("saved-posts/{postID}")]
		public async Task<IActionResult> SavePost(string postID)
		{
			var result = await _userService.SavePost(User, postID);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("saved-posts/remove/{postID}")]
		public async Task<IActionResult> UnsavePost(string postID)
		{
			var result = await _userService.UnsavePost(User, postID);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("saved-posts")]
		public async Task<IActionResult> GetSavedPosts()
		{
			var result = await _userService.GetSavedPosts(User);

			return result.IsSuccess ? Ok(result.Value.Select(p => p.ToMinimalDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}


		[HttpPost("add-search/{username}")]
		public async Task<IActionResult> AddSearch(string username)
		{
			var result = await _userService.AddToRecentSearches(User, username);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("remove-search/{username}")]
		public async Task<IActionResult> RemoveSearch(string username)
		{
			var result = await _userService.RemoveFromRecentSearches(User, username);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("clear-search")]
		public async Task<IActionResult> ClearSearch()
		{
			var result = await _userService.ClearRecentSearches(User);

			return result.IsSuccess ? NoContent() : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("get-search")]
		public async Task<IActionResult> GetSearches()
		{
			var result = await _userService.GetRecentSearches(User);

			return result.IsSuccess ? Ok(result.ValueOrDefault.Select(u => u.ToMinimalDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}
	}
}
