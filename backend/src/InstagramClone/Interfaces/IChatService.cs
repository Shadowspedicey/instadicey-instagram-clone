using FluentResults;
using InstagramClone.Data.Entities;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface IChatService
	{
		public Task<Result<ChatRoom>> GetOrCreateChatRoom(ClaimsPrincipal currentUserPrinciple, string[] usernames);
		public Task<Result<ChatRoom>> GetRoom(ClaimsPrincipal currentUserPrinciple, string? username = null, string? roomID = null);
		public Task<Result<ICollection<ChatRoom>>> GetUserRooms(ClaimsPrincipal currentUserPrinciple);
		public Task<Result<ICollection<Message>>> GetMessages(ClaimsPrincipal currentUserPrinciple, string roomID);
		public Task<Result<Message>> SendMessage(ClaimsPrincipal currentUserPrinciple, string roomID, string message, string fileDownloadEndpoint = "");
	}
}
