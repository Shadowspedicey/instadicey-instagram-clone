using FluentResults;
using InstagramClone.Data.Entities;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface IChatService
	{
		public Task<Result<ChatRoom>> CreateChatRoom(ClaimsPrincipal currentUserPrinciple, string[] usernames);
		public Task<Result<ChatRoom>> GetRoomWithUser(ClaimsPrincipal currentUserPrinciple, string username);
		public Task<Result<ICollection<ChatRoom>>> GetUserRooms(ClaimsPrincipal currentUserPrinciple);
		public Task<Result<ICollection<Message>>> GetMessages(ClaimsPrincipal currentUserPrinciple, string roomID);
		public Task<Result<Message>> SendMessage(ClaimsPrincipal currentUserPrinciple, string roomID, string message);
	}
}
