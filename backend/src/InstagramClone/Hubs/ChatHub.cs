using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InstagramClone.Hubs
{
	[Authorize]
	public class ChatHub(AppDbContext dbContext, IChatService chatService) : Hub
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IChatService _chatService = chatService;
		private string DownloadFileEndpoint
		{
			get
			{
				var httpContext = Context.GetHttpContext();
				return $"{httpContext!.Request.Scheme}://{httpContext.Request.Host}/file/";
			}
		}
		public async Task SendMessage(string message)
		{
			string? roomID = Context.GetHttpContext()!.Request.Query["roomID"];
			if (roomID is null) return;
			await _chatService.SendMessage(Context.User!, roomID, message, DownloadFileEndpoint);
		}

		public override async Task OnConnectedAsync()
		{
			if (Context.User is null) return;

			string? roomID = Context.GetHttpContext()!.Request.Query["roomID"];
			if (roomID is null) return;

			ChatRoom? chatRoom = await _dbContext.ChatRooms.FindAsync(roomID);
			if (chatRoom is null) return;

			if (!chatRoom.Users.Any(u => u.Id == Context.User.FindFirstValue("sub"))) return;

			string connectionId = Context.ConnectionId;
			await Groups.AddToGroupAsync(connectionId, roomID);

			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			string? roomID = Context.GetHttpContext()!.Request.Query["roomID"];
			if (roomID is null) return;

			var connectionId = Context.ConnectionId;
		
			await Groups.RemoveFromGroupAsync(connectionId, roomID);

			await base.OnDisconnectedAsync(exception);
		}
	}
}
