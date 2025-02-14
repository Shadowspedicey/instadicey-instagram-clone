using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Security.Claims;

namespace InstagramClone.Tests.UnitTests
{
	public class UserServiceTests : IDisposable
	{
		private readonly AppDbContext _dbContext;
		private readonly UserManager<User> _userManager;
		private readonly User _user;
		private readonly ClaimsPrincipal _claimsPrincipal;
		public UserServiceTests()
		{
			SqliteConnection sqliteConnection = new SqliteConnection("DataSource=:memory:");
			sqliteConnection.Open();
			var dbOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(sqliteConnection).Options;

			AppDbContext dbContext = new(dbOptions);
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

			var userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
			userManagerMock
				.Setup(manager => manager.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>()).Result)
				.Callback<User, string>((user, username) =>
				{
					user.UserName = username;
					user.NormalizedUserName = username.ToUpper();
					dbContext.SaveChanges();
				})
				.Returns(IdentityResult.Success);

			_dbContext = dbContext;
			_userManager = userManagerMock.Object;
			_user = user;

			var claims = new List<Claim>() { new ("sub", user.Id) };
			var claimsIdentity = new ClaimsIdentity(claims);
			_claimsPrincipal = new(claimsIdentity);
		}

		public void Dispose()
		{
			_dbContext.Dispose();
			GC.SuppressFinalize(this);
		}

		[Fact]
		public async Task GetUser_ShouldReturnUser_WhenUserExists()
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.GetUser("exampleusername");

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.ValueOrDefault);
		}

		[Fact]
		public async Task GetUser_ShouldReturnResultFailedWithNotFoundErrorCode_WhenUserDoesntExists()
		{
			UserService userSerice = new(_dbContext, null!, null!, null!);

			var result = await userSerice.GetUser("nonexistingusername");

			Assert.False(result.IsSuccess);
			Assert.Null(result.ValueOrDefault);
		}

		[Fact]
		public async Task ChangeUsername_ShouldUpdateUsername_WhenUsernameDoesntCollide()
		{
			await _dbContext.AddAsync(new User()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			});
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, _userManager, null!, null!);

			var result = await userService.ChangeUsername(_user, "NewUsername");
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Equal("newusername", _user.UserName);
			Assert.Equal("NEWUSERNAME", _user.NormalizedUserName);
		}

		[InlineData("anotheruser")]
		// Makes sure it's case in-sensitive
		[InlineData("AnotherUser")]
		[Theory]
		public async Task ChangeUsername_ShouldReturnResultFailedWithErrorCodeDuplicate_WhenUsernameIsAlreadyTaken(string newUsername)
		{
			await _dbContext.AddAsync(new User()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			});
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, _userManager, null!, null!);

			var result = await userService.ChangeUsername(_user, newUsername);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors[0].Metadata["code"]);
			Assert.Equal("exampleusername", _user.UserName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("Ahmed Khaled")]
		[InlineData("Ahmed")]
		public async Task ChangeRealName_ShouldUpdateRealname_WhenValid(string? newRealName)
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.ChangeRealName(_user, newRealName);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Equal(newRealName, _user.RealName);
		}

		[Theory]
		[InlineData("")]
		[InlineData("  ")]
		public async Task ChangeRealName_ShouldSetNameToNull_WhenEmptyString(string? emptyRealName)
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.ChangeRealName(_user, emptyRealName);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Null(_user.RealName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("This is a random bio.")]
		[InlineData("This is a random bio example again.")]
		public async Task ChangeBio_ShouldUpdateBio_WhenValidData(string? bio)
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.ChangeBio(_user, bio);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Equal(bio, _user.Bio);
		}

		[Theory]
		[InlineData("")]
		[InlineData(" ")]
		public async Task ChangeBio_ShouldSetBioToNull_WhenEmptyString(string? emptyBio)
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.ChangeBio(_user, emptyBio);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Null(_user.Bio);
		}

		// Following functionality

		[Fact]
		public async Task FollowUser_ShouldReturnSuccess_WhenFollowedUserExistsAndIsntAlreadyFollowed()
		{
			User otherUser = new()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.FollowUser(_claimsPrincipal, "anotheruser");
			await _dbContext.Entry(_user).ReloadAsync();
			await _dbContext.Entry(otherUser).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.True(_user.Following.Contains(otherUser));
			Assert.True(otherUser.Followers.Contains(_user));
		}

		[Fact]
		public async Task FollowUser_ShouldReturnResultFailedWithErrorCodeDuplicate_WhenFollowedUserExistsAndIsAlreadyFollowed()
		{
			User otherUser = new()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [_user],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser);
			_user.Following.Add(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.FollowUser(_claimsPrincipal, "anotheruser");
			await _dbContext.Entry(_user).ReloadAsync();
			await _dbContext.Entry(otherUser).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
			Assert.Single(_user.Following);
			Assert.Single(otherUser.Followers);
		}

		[Fact]
		public async Task FollowUser_ShouldReturnResultFailedWithErrorCodeNotFound_WhenFollowedUserDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.FollowUser(_claimsPrincipal, "anotheruser");
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
			Assert.Empty(_user.Following);
		}

		[Fact]
		public async Task UnfollowUser_ShouldReturnSuccess_WhenUserExistsAndIsAlreadyFollowed()
		{
			User otherUser = new()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [_user],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser);
			_user.Following.Add(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnfollowUser(_claimsPrincipal, "anotheruser");
			await _dbContext.Entry(_user).ReloadAsync();
			await _dbContext.Entry(otherUser).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_user.Following);
			Assert.Empty(otherUser.Followers);
		}

		[Fact]
		public async Task UnfollowUser_ShouldReturnResultFailed_WhenUserExistsAndIsntFollowed()
		{
			User otherUser = new()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnfollowUser(_claimsPrincipal, "anotheruser");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task UnfollowUser_ShouldReturnResultFailed_WhenUserDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnfollowUser(_claimsPrincipal, "anotheruser");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		// Posts saving functionality

		[Fact]
		public async Task SavePost_ShouldReturnSuccess_WhenPostExistsAndIsntSaved()
		{
			Post post = new()
			{
				Caption = "Caption",
				Photo = "",
				User = _user,
				CreatedAt = DateTime.Now,
			};
			await _dbContext.Posts.AddAsync(post);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.SavePost(_claimsPrincipal, post.ID);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.True(_user.SavedPosts.Contains(post));
		}

		[Fact]
		public async Task SavePost_ShouldReturnFailedResult_WhenPostExistsAndAlreadySaved()
		{
			Post post = new()
			{
				Caption = "Caption",
				Photo = "",
				User = _user,
				CreatedAt = DateTime.Now,
			};
			await _dbContext.Posts.AddAsync(post);
			_user.SavedPosts.Add(post);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.SavePost(_claimsPrincipal, post.ID);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
			Assert.Single(_user.SavedPosts);
		}

		[Fact]
		public async Task SavePost_ShouldReturnFailedResult_WhenPostDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.SavePost(_claimsPrincipal, Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task UnsavePost_ShouldReturnSuccess_WhenPostExistsAndIsAlreadySaved()
		{
			Post post = new()
			{
				Caption = "Caption",
				Photo = "",
				User = _user,
				CreatedAt = DateTime.Now,
			};
			await _dbContext.Posts.AddAsync(post);
			_user.SavedPosts.Add(post);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnsavePost(_claimsPrincipal, post.ID);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_user.SavedPosts);
		}

		[Fact]
		public async Task UnsavePost_ShouldReturnFailedResult_WhenPostExistsAndIsntSaved()
		{
			Post post = new()
			{
				Caption = "Caption",
				Photo = "",
				User = _user,
				CreatedAt = DateTime.Now,
			};
			await _dbContext.Posts.AddAsync(post);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnsavePost(_claimsPrincipal, post.ID);

			Assert.False(result.IsSuccess);
			Assert.Equal(ErrorCode.Duplicate, result.Errors.First().Metadata["error"]);
		}

		[Fact]
		public async Task UnsavePost_ShouldReturnFailedResult_WhenPostDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!, null!);

			var result = await userService.UnsavePost(_claimsPrincipal, Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Equal(ErrorCode.NotFound, result.Errors.First().Metadata["error"]);
		}
	}
}
