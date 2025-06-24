using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
				EmailConfirmed = true,
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

		private async Task<User> AddAnotherUser()
		{
			User otherUser = new()
			{
				Email = "anotherexample@domain.com",
				EmailConfirmed = true,
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

			return otherUser;
		}

		[Fact]
		public async Task GetUser_ShouldReturnUser_WhenUserExists()
		{
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.GetUser("exampleusername");

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.ValueOrDefault);
		}

		[Fact]
		public async Task GetUser_ShouldReturnResultFailedWithNotFoundErrorCode_WhenUserDoesntExists()
		{
			UserService userSerice = new(_dbContext, null!, null!);

			var result = await userSerice.GetUser("nonexistingusername");

			Assert.False(result.IsSuccess);
			Assert.Null(result.ValueOrDefault);
		}

		[Fact]
		public async Task SearchForUsers_ShouldReturnProperUsers2()
		{
			User anotherUser = new()
			{
				Email = "anotherexample@domain.com",
				EmailConfirmed = true,
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User friendlyUser = new()
			{
				Email = "friendlyuser@domain.com",
				EmailConfirmed = true,
				UserName = "frienuserdly",
				NormalizedUserName = "FRIENUSERDLY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User randomGuy = new()
			{
				Email = "randomguy@domain.com",
				UserName = "randomguy",
				EmailConfirmed = true,
				NormalizedUserName = "RANDOMGUY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			await _dbContext.AddRangeAsync(anotherUser, friendlyUser, randomGuy);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.SearchForUsers("guy");

			Assert.True(result.IsSuccess);
			var users = result.Value;
			Assert.Single(users);
			Assert.Contains(users, u => u.UserName == "randomguy");
		}

		[Fact]
		public async Task SearchForUsers_ShouldReturnProperUsers3()
		{
			User anotherUser = new()
			{
				Email = "anotherexample@domain.com",
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User friendlyUser = new()
			{
				Email = "friendlyuser@domain.com",
				UserName = "frienuserdly",
				NormalizedUserName = "FRIENUSERDLY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User randomGuy = new()
			{
				Email = "randomguy@domain.com",
				UserName = "randomguy",
				NormalizedUserName = "RANDOMGUY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			await _dbContext.AddRangeAsync(anotherUser, friendlyUser, randomGuy);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.SearchForUsers("usernae");

			Assert.True(result.IsSuccess);
			var users = result.Value;
			Assert.Empty(users);
		}

		[Fact]
		public async Task SearchForUsers_ShouldReturnProperUsers()
		{
			User anotherUser = new()
			{
				Email = "anotherexample@domain.com",
				EmailConfirmed = true,
				UserName = "anotheruser",
				NormalizedUserName = "ANOTHERUSER",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User friendlyUser = new()
			{
				Email = "friendlyuser@domain.com",
				EmailConfirmed = true,
				UserName = "frienuserdly",
				NormalizedUserName = "FRIENUSERDLY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			User randomGuy = new()
			{
				Email = "randomguy@domain.com",
				EmailConfirmed = true,
				UserName = "randomguy",
				NormalizedUserName = "RANDOMGUY",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			await _dbContext.Users.AddRangeAsync(anotherUser, friendlyUser, randomGuy);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.SearchForUsers("user");

			Assert.True(result.IsSuccess);
			var users = result.Value;
			Assert.Equal(3, users.Count);
			Assert.Contains(users, u => u.UserName == anotherUser.UserName);
			Assert.Contains(users, u => u.UserName == friendlyUser.UserName);
			Assert.Contains(users, u => u.UserName == _user.UserName);
		}

		[Fact]
		public async Task ChangeUsername_ShouldUpdateUsername_WhenUsernameDoesntCollide()
		{
			await AddAnotherUser();
			UserService userService = new(_dbContext, _userManager, null!);

			var result = await userService.ChangeUsername(_user, "newusername");
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Equal("newusername", _user.UserName);
			Assert.Equal("NEWUSERNAME", _user.NormalizedUserName);
		}

		[InlineData("anotheruser")]
		//[InlineData("AnotherUser")]–Handled in controller filter now
		[Theory]
		public async Task ChangeUsername_ShouldReturnResultFailedWithErrorCodeDuplicate_WhenUsernameIsAlreadyTaken(string newUsername)
		{
			await AddAnotherUser();
			UserService userService = new(_dbContext, _userManager, null!);

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
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.ChangeBio(_user, emptyBio);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Null(_user.Bio);
		}

		// Following functionality

		[Fact]
		public async Task FollowUser_ShouldReturnSuccess_WhenFollowedUserExistsAndIsntAlreadyFollowed()
		{
			User otherUser = await AddAnotherUser();
			UserService userService = new(_dbContext, null!, null!);

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
			User otherUser = await AddAnotherUser();
			otherUser.Followers.Add(_user);
			_user.Following.Add(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.FollowUser(_claimsPrincipal, "anotheruser");
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
			Assert.Empty(_user.Following);
		}

		[Fact]
		public async Task UnfollowUser_ShouldReturnSuccess_WhenUserExistsAndIsAlreadyFollowed()
		{
			User otherUser = await AddAnotherUser();
			_user.Following.Add(otherUser);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

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
			User otherUser = await AddAnotherUser();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.UnfollowUser(_claimsPrincipal, "anotheruser");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task UnfollowUser_ShouldReturnResultFailed_WhenUserDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.SavePost(_claimsPrincipal, post.ID);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
			Assert.Single(_user.SavedPosts);
		}

		[Fact]
		public async Task SavePost_ShouldReturnFailedResult_WhenPostDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

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
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.UnsavePost(_claimsPrincipal, post.ID);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task UnsavePost_ShouldReturnFailedResult_WhenPostDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.UnsavePost(_claimsPrincipal, Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}

		// Recent searches

		[Fact]
		public async Task AddToRecentSearches_ShouldReturnSuccess_WhenSearchedUserExists()
		{
			User otherUser = await AddAnotherUser();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName!);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Contains(_user.RecentSearches, s => s.SearchedUser.UserName == otherUser.UserName);
		}

		[Fact]
		public async Task AddToRecentSearches_ShouldReturnFailedResultWithErrorCodeNotFound_WhenSearchedUserDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.AddToRecentSearches(_claimsPrincipal, "anotheruser");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors[0].Metadata["code"]);
		}

		[Fact]
		public async Task AddToRecentSearches_ShouldNotAddUserAgain_WhenAlreadyInList()
		{
			User otherUser = await AddAnotherUser();
			_dbContext.UserSearches.Add(new() { User = _user, SearchedUser = otherUser });
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName!);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Single(_user.RecentSearches);
		}

		[Fact]
		public async Task AddToRecentSearches_ShouldPutUserAtStartOfList_WhenAlreadyInListAndWasSearchedBeforeAnotherUser()
		{
			User otherUser = await AddAnotherUser();
			User otherUser2 = new()
			{
				Email = "anotherexample2@domain.com",
				EmailConfirmed = true,
				UserName = "anotheruser2",
				NormalizedUserName = "ANOTHERUSER2",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser2);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName);
			await userService.AddToRecentSearches(_claimsPrincipal, otherUser2.UserName);
			await _dbContext.Entry(_user).ReloadAsync();
			Assert.NotEqual(otherUser.UserName, _user.RecentSearches.OrderByDescending(us => us.SearchedAt).First().SearchedUser.UserName);
			var result = await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName!);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Equal(2, _user.RecentSearches.Count);
			Assert.Equal(otherUser.UserName, _user.RecentSearches.OrderByDescending(us => us.SearchedAt).First().SearchedUser.UserName);
		}

		[Fact]
		public async Task RemoveFromRecentSearches_ShouldReturnSuccess_WhenSearchedUserExists()
		{
			User otherUser = await AddAnotherUser();
			_user.RecentSearches.Add(new() { User = _user, SearchedUser = otherUser });
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.RemoveFromRecentSearches(_claimsPrincipal, otherUser.UserName!);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_user.RecentSearches);
		}

		[Fact]
		public async Task RemoveFromRecentSearches_ShouldReturnFailedResultWithErrorCodeNotFound_WhenSearchedUserDoesntExist()
		{
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.RemoveFromRecentSearches(_claimsPrincipal, "anotheruser");

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors[0].Metadata["code"]);
		}

		[Fact]
		public async Task RemoveFromRecentSearches_ShouldReturnFailedResultWithErrorCodeDuplicate_WhenSearchedUserExistsAndIsNotInList()
		{
			User otherUser = await AddAnotherUser();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.RemoveFromRecentSearches(_claimsPrincipal, otherUser.UserName!);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors[0].Metadata["code"]);
		}

		[Fact]
		public async Task ClearRecentSearches_ShouldReturnSuccess_IfListIsEmpty()
		{
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.ClearRecentSearches(_claimsPrincipal);

			Assert.True(result.IsSuccess);
		}

		[Fact]
		public async Task ClearRecentSearches_ShouldReturnSuccess_IfListHasOneSearch()
		{
			User otherUser = await AddAnotherUser();
			_user.RecentSearches.Add(new() { User = _user, SearchedUser = otherUser });
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.ClearRecentSearches(_claimsPrincipal);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_user.RecentSearches);
		}

		[Theory]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(5)]
		public async Task ClearRecentSearches_ShouldReturnSuccess_IfListHasManySearches(int n)
		{
			for (int i = 0; i < n; i++)
			{
				var u = new User
				{
					UserName = $"anotheruser{i}",
					Email = $"anotheruser{i}@domain.com",
					IsVerified = false,
					Followers = [],
					Following = [],
					RecentSearches = [],
					CreatedAt = DateTime.Now,
					LastLogin = DateTime.Now,
				};
				await _dbContext.Users.AddAsync(u);
				_user.RecentSearches.Add(new() { User = _user, SearchedUser = u });
			}
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);

			var result = await userService.ClearRecentSearches(_claimsPrincipal);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_user.RecentSearches);
		}

		[Fact]
		public async Task GetRecentSearches_ShouldReturnSuccess()
		{
			User otherUser = await AddAnotherUser();
			User otherUser2 = new()
			{
				Email = "anotherexample2@domain.com",
				UserName = "anotheruser2",
				NormalizedUserName = "ANOTHERUSER2",
				IsVerified = false,
				LastLogin = DateTime.Now,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.Now,
			};
			await _dbContext.AddAsync(otherUser2);
			await _dbContext.SaveChangesAsync();
			UserService userService = new(_dbContext, null!, null!);
			await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName);
			await userService.AddToRecentSearches(_claimsPrincipal, otherUser2.UserName);
			await userService.AddToRecentSearches(_claimsPrincipal, otherUser.UserName);

			var result = await userService.GetRecentSearches(_claimsPrincipal);
			await _dbContext.Entry(_user).ReloadAsync();

			Assert.Equal(otherUser.UserName, _user.RecentSearches.First().SearchedUser.UserName);
		}
	}
}
