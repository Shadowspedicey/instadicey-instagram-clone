using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace InstagramClone.Tests.UnitTests
{
	public class ChatServiceTests : IDisposable
	{
		private readonly AppDbContext _dbContext;
		private readonly User _user;
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly ChatService _chatService;

		public ChatServiceTests()
		{
			SqliteConnection sqliteConnection = new("DataSource=:memory:");
			sqliteConnection.Open();
			DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(sqliteConnection).Options;
			AppDbContext dbContext = new(options);
			dbContext.Database.EnsureCreated();

			User user = new()
			{
				Email = "example@domain.com",
				UserName = "exampleusername",
				NormalizedUserName = "EXAMPLEUSERNAME",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
			dbContext.Users.Add(user);
			dbContext.SaveChanges();

			ChatService chatService = new(dbContext, null!);

			_dbContext = dbContext;
			_user = user;
			_chatService = chatService;

			var claims = new List<Claim>() { new("sub", user.Id) };
			var claimsIdentity = new ClaimsIdentity(claims);
			_claimsPrincipal = new(claimsIdentity);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_dbContext.Dispose();
		}

		private User GetAnotherUser(string id = "")
		{
			return new()
			{
				Email = $"anotherexample{id}@domain.com",
				UserName = $"anotheruser{id}",
				NormalizedUserName = $"ANOTHERUSER{id.ToUpper()}",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
		}

		[Fact]
		public async Task CreateChatRoom_ShouldReturnSuccess_IfProvidedOneValidPerson()
		{
			User anotherUser = GetAnotherUser();
			await _dbContext.AddAsync(anotherUser);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.CreateChatRoom(_claimsPrincipal, [anotherUser.UserName!]);

			Assert.True(result.IsSuccess);
			Assert.Single(_dbContext.ChatRooms);
			ChatRoom chatRoom = await _dbContext.ChatRooms.FirstAsync();
			Assert.Contains(chatRoom.Users, r => r.UserName == _user.UserName);
			Assert.Contains(chatRoom.Users, r => r.UserName == anotherUser.UserName);
		}

		[Fact]
		public async Task CreateChatRoom_ShouldReturnFailedResultInvalidInput_IfProvidedOnePersonThatDoesntExist()
		{
			var result = await _chatService.CreateChatRoom(_claimsPrincipal, ["anotheruser"]);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InvalidInput), result.Errors.First().Metadata["code"]);
			Assert.Empty(_dbContext.ChatRooms);
		}

		[Fact]
		public async Task CreateChatRoom_ShouldReturnSuccess_IfProvidedMultipleValidPeople()
		{
			User anotherUser = GetAnotherUser();
			User anotherUser2 = GetAnotherUser("2");
			await _dbContext.AddRangeAsync(anotherUser, anotherUser2);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.CreateChatRoom(_claimsPrincipal, [anotherUser.UserName!, anotherUser2.UserName!]);

			Assert.True(result.IsSuccess);
			Assert.Single(_dbContext.ChatRooms);
			ChatRoom chatRoom = await _dbContext.ChatRooms.FirstAsync();
			Assert.Contains(chatRoom.Users, r => r.UserName == _user.UserName);
			Assert.Contains(chatRoom.Users, r => r.UserName == anotherUser.UserName);
			Assert.Contains(chatRoom.Users, r => r.UserName == anotherUser2.UserName);
		}

		[Fact]
		public async Task CreateChatRoom_ShouldReturnFailedResultNotFound_IfProvidedMultiplePeopleWithAtLeastOneNotExisting()
		{
			User anotherUser = GetAnotherUser();
			await _dbContext.AddAsync(anotherUser);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.CreateChatRoom(_claimsPrincipal, [anotherUser.UserName!, "anotheruser2"]);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InvalidInput), result.Errors.First().Metadata["code"]);
			Assert.Empty(_dbContext.ChatRooms);
		}

		[Fact]
		public async Task GetRoomWithUser_ShouldReturnSuccess_IfUserAndRoomExist()
		{
			User anotherUser = GetAnotherUser();
			ChatRoom chatRoom = new() { Users = [_user, anotherUser] };
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.GetRoomWithUser(_claimsPrincipal, anotherUser.UserName!);

			Assert.True(result.IsSuccess);
			Assert.Equal(chatRoom.ID, result.Value.ID);
		}

		[Fact]
		public async Task GetRoomWithUser_ShouldReturnFailedInvalidInput_IfUserDoesntExist()
		{
			var result = await _chatService.GetRoomWithUser(_claimsPrincipal, "anotheruser2");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InvalidInput), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task GetRoomWithUser_ShouldReturnFailed404_IfRoomDoesntExist()
		{
			User anotherUser = GetAnotherUser();
			await _dbContext.AddAsync(anotherUser);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.GetRoomWithUser(_claimsPrincipal, anotherUser.UserName!);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task GetUserRooms_ShouldReturnSuccessEmptyList_WhenUserDoesntHaveChats()
		{
			var result = await _chatService.GetUserRooms(_claimsPrincipal);

			Assert.True(result.IsSuccess);
			Assert.Empty(result.Value);
		}

		[Fact]
		public async Task GetUserRooms_ShouldReturnSuccessOrderedChats_WhenUserHasChats()
		{
			User anotherUser = GetAnotherUser();
			User anotherUser2 = GetAnotherUser("2");
			ChatRoom chatRoom = new() { Users = [_user, anotherUser] };
			ChatRoom chatRoom2 = new() { Users = [_user, anotherUser2] };
			Message message = new() { Content = "message", User = _user };
			Message message2 = new() { Content = "message2", User = _user };
			chatRoom.Messages.Add(message);
			chatRoom2.Messages.Add(message2);
			await _dbContext.AddRangeAsync(chatRoom, chatRoom2);
			await _dbContext.SaveChangesAsync();

			var result = await _chatService.GetUserRooms(_claimsPrincipal);

			Assert.True(result.IsSuccess);
			Assert.Equal(chatRoom2.ID, result.Value.First().ID);
			Assert.Equal(2, result.Value.Count);
		}

		[Fact]
		public async Task GetMessages_ShouldReturnSuccessEmptyList_WhenRoomExistsButHasNoMsgs()
		{
			User anotherUser = GetAnotherUser();
			ChatRoom chatRoom = new() { Users = [_user, anotherUser] };
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, chatRoom, "CanAccessRoomMessages").Result).Returns(AuthorizationResult.Success());
			ChatService chatService = new(_dbContext, authorizationServiceMock.Object);

			var result = await chatService.GetMessages(_claimsPrincipal, chatRoom.ID);

			Assert.True(result.IsSuccess);
			Assert.Empty(result.Value);
		}

		[Fact]
		public async Task GetMessages_ShouldReturnSuccessOrderedMessages_WhenRoomExists()
		{
			User anotherUser = GetAnotherUser();
			ChatRoom chatRoom = new() { Users = [_user, anotherUser] };
			Message message = new() { Content = "message", User = _user };
			Message message2 = new() { Content = "message2", User = anotherUser };
			chatRoom.Messages.Add(message2);
			chatRoom.Messages.Add(message);
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, chatRoom, "CanAccessRoomMessages").Result).Returns(AuthorizationResult.Success());
			ChatService chatService = new(_dbContext, authorizationServiceMock.Object);

			var result = await chatService.GetMessages(_claimsPrincipal, chatRoom.ID);

			Assert.True(result.IsSuccess);
			Assert.Equal(2, result.Value.Count);
			Assert.Equal(message2.ID, result.Value.First().ID);
		}

		[Fact]
		public async Task GetMessages_ShouldReturnFailed404_WhenRoomDoesntExist()
		{
			var result = await _chatService.GetMessages(_claimsPrincipal, Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task GetMessages_ShouldReturnFailedInsufficientPermissions_WhenRoomExistsButUserIsntInIt()
		{
			User anotherUser = GetAnotherUser();
			User anotherUser2 = GetAnotherUser("2");
			ChatRoom chatRoom = new() { Users = [anotherUser, anotherUser2] };
			Message message = new() { Content = "message", User = anotherUser };
			chatRoom.Messages.Add(message);
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, chatRoom, "CanAccessRoomMessages").Result).Returns(AuthorizationResult.Failed());
			ChatService chatService = new(_dbContext, authorizationServiceMock.Object);

			var result = await chatService.GetMessages(_claimsPrincipal, chatRoom.ID);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InsufficientPermissions), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task SendMessage_ShouldReturnSuccess_WhenRoomExistsAndUserIsInIt()
		{
			User anotherUser = GetAnotherUser();
			ChatRoom chatRoom = new() { Users = [_user, anotherUser] };
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, chatRoom, "CanAccessRoomMessages").Result).Returns(AuthorizationResult.Success());
			ChatService chatService = new(_dbContext, authorizationServiceMock.Object);

			string message = "msg";
			var result = await chatService.SendMessage(_claimsPrincipal, chatRoom.ID, message);
			await _dbContext.Entry(chatRoom).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Single(chatRoom.Messages);
			Assert.Equal(message, chatRoom.Messages.First().Content);
		}

		[Fact]
		public async Task SendMessage_ShouldReturn404_WhenDoesntRoomExist()
		{
			var result = await _chatService.SendMessage(_claimsPrincipal, Ulid.NewUlid().ToString(), "msg");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task SendMessage_ShouldReturnInsufficientPermissions_WhenRoomExistsButUserIsntInIt()
		{
			User anotherUser = GetAnotherUser();
			User anotherUser2 = GetAnotherUser("2");
			ChatRoom chatRoom = new() { Users = [anotherUser, anotherUser2] };
			await _dbContext.AddAsync(chatRoom);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, chatRoom, "CanAccessRoomMessages").Result).Returns(AuthorizationResult.Failed());
			ChatService chatService = new(_dbContext, authorizationServiceMock.Object);

			var result = await chatService.SendMessage(_claimsPrincipal, chatRoom.ID, "message");
			await _dbContext.Entry(chatRoom).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InsufficientPermissions), result.Errors.First().Metadata["code"]);
			Assert.Empty(chatRoom.Messages);
		}
	}
}
