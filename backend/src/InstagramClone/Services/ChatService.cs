using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class ChatService(AppDbContext dbContext, IAuthorizationService authorizationService) : IChatService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IAuthorizationService _authorizationService = authorizationService;

		public Task<Result<ChatRoom>> CreateChatRoom(ClaimsPrincipal currentUserPrinciple, string[] usernames)
		{
			throw new NotImplementedException();
		}

		public Task<Result<ChatRoom>> GetRoomWithUser(ClaimsPrincipal currentUserPrinciple, string username)
		{
			throw new NotImplementedException();
		}

		public Task<Result<ICollection<ChatRoom>>> GetUserRooms(ClaimsPrincipal currentUserPrinciple)
		{
			throw new NotImplementedException();
		}

		public Task<Result<ICollection<Message>>> GetMessages(ClaimsPrincipal currentUserPrinciple, string roomID)
		{
			throw new NotImplementedException();
		}

		public Task<Result<Message>> SendMessage(ClaimsPrincipal currentUserPrinciple, string roomID, string message)
		{
			throw new NotImplementedException();
		}
	}
}
