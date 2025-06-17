using FluentResults;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Controllers
{
	[Route("[controller]")]
	[Authorize("IsNotGuest")]
	[ApiController]
	public class ChatController(IChatService chatService) : ControllerBase
	{
		private readonly IChatService _chatService = chatService;
		private string DownloadFileEndpoint => $"{Request.Scheme}://{Request.Host}/file/";

		[HttpGet("room")]
		public async Task<IActionResult> GetRoom(string? roomID, string? username)
		{
			if (roomID is null && username is null)
				return this.ProblemWithErrors(statusCode: 400, detail: "A room was requested without providing a roomID or a username", errors: null!);

			var result = roomID is not null ? await _chatService.GetRoom(User, roomID: roomID) : await _chatService.GetRoom(User, username: username);
			return result.IsSuccess ? Ok(result.Value.ToViewDTO(DownloadFileEndpoint)) : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("rooms")]
		public async Task<IActionResult> GetRooms()
		{
			var result = await _chatService.GetUserRooms(User);

			return result.IsSuccess ? Ok(result.Value.Select(cr => cr.ToViewDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpGet("messages")]
		public async Task<IActionResult> GetMessages(string roomID)
		{
			var result = await _chatService.GetMessages(User, roomID);

			return result.IsSuccess ? Ok(result.Value.Select(m => m.ToViewDTO(DownloadFileEndpoint))) : this.AppropriateResponseBasedOnResult(result);
		}

		[HttpPost("room")]
		public async Task<IActionResult> CreateRoom([FromBody] string[] usernames)
		{
			var result = await _chatService.GetOrCreateChatRoom(User, usernames);

			return result.IsSuccess ? Ok(result.Value.ToViewDTO(DownloadFileEndpoint)) : this.AppropriateResponseBasedOnResult(result);
		}
	}
}
