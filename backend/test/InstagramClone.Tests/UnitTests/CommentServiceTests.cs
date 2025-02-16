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
	public class CommentServiceTests : IDisposable
	{
		private readonly AppDbContext _dbContext;
		private readonly User _user;
		private readonly Post _post;
		private readonly ClaimsPrincipal _claimsPrincipal;
		public CommentServiceTests()
		{
			SqliteConnection sqliteConnection = new SqliteConnection("DataSource=:memory:");
			sqliteConnection.Open();
			DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
				.UseSqlite(sqliteConnection)
				.Options;

			AppDbContext dbContext = new(options);
			dbContext.Database.EnsureCreated();
			User user = new()
			{
				Email = "example@domain.com",
				UserName = "exampleusername",
				IsVerified = false,
				LastLogin = DateTime.UtcNow,
				RecentSearches = [],
				LikedPosts = [],
				Following = [],
				Followers = [],
				CreatedAt = DateTime.UtcNow,
			};
			Post post = new() { Photo = "", CreatedAt = DateTime.UtcNow, User = user, Comments = [] };
			dbContext.Add(user);
			dbContext.Add(post);
			dbContext.SaveChanges();

			_dbContext = dbContext;
			_user = user;
			_post = post;

			IEnumerable<Claim> claims = [new Claim("sub", user.Id)];
			ClaimsIdentity identity = new ClaimsIdentity(claims);
			ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
			_claimsPrincipal = claimsPrincipal;
		}

		public void Dispose()
		{
			_dbContext.Dispose();
			GC.SuppressFinalize(this);
		}

		private Comment GetComment()
		{
			return new()
			{
				Content = "Content",
				Post = _post,
				User = _user
			};
		}

		// TODO: Check if the comment was actually added
		[Fact]
		public async Task AddComment_ShouldReturnSuccess_WhenPostExists()
		{
			CommentService commentService = new(_dbContext, null!);

			var result = await commentService.AddComment(_claimsPrincipal, _post, "Comment.");
			await _dbContext.Entry(_post).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Single(_post.Comments);
			Assert.Equal("Comment.", _post.Comments.First().Content);
		}
		
		[Fact]
		public async Task AddComment_ShouldRetainOrder_WhenMultipleCommentsPosted()
		{
			CommentService commentService = new(_dbContext, null!);

			await commentService.AddComment(_claimsPrincipal, _post, "Comment.");
			await commentService.AddComment(_claimsPrincipal, _post, "Comment 2.");
			await _dbContext.Entry(_post).ReloadAsync();

			Assert.Equal("Comment 2.", _post.SortedComments.First().Content);
		}

		//[Fact]
		//public async Task AddComment_ShouldReturnFailedResultWithErrorCodeNotFound_WhenPostDoesntExist()
		//{
		//	var postMock = Mock.Of<Post>();
		//	postMock.ID = Ulid.NewUlid().ToString();
		//	postMock.Comments = [];
		//	CommentService commentService = new(_dbContext, null!);

		//	var result = await commentService.AddComment(_claimsPrincipal, postMock, "Comment.");

		//	Assert.False(result.IsSuccess);
		//	Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		//}

		[Fact]
		public async Task DeleteComment_ShouldReturnSuccess_WhenCommentExistsAndUserIsOwner()
		{
			Comment comment = GetComment();
			_post.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, comment, "CanDeleteComment").Result).Returns(AuthorizationResult.Success);
			CommentService commentService = new(_dbContext, authorizationServiceMock.Object);

			var result = await commentService.DeleteComment(_claimsPrincipal, comment.ID);

			Assert.True(result.IsSuccess);
			Assert.Empty(_post.Comments);
		}

		[Fact]
		public async Task DeleteComment_ShouldReturnSuccess_WhenCommentExistsAndUserIsPostOwner()
		{
			User otherUser = new()
			{
				IsVerified = false,
				UserName = "otheruser",
				Following = [],
				Followers = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			Comment comment = new()
			{
				Content = "Another comment.",
				User = otherUser,
				Post = _post
			};
			await _dbContext.AddAsync(otherUser);
			_post.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, comment, "CanDeleteComment").Result).Returns(AuthorizationResult.Success);
			CommentService commentService = new(_dbContext, authorizationServiceMock.Object);

			var result = await commentService.DeleteComment(_claimsPrincipal, comment.ID);

			Assert.True(result.IsSuccess);
			Assert.Empty(_post.Comments);
		}

		[Fact]
		public async Task DeleteComment_ShouldReturnFailedResultWithErrorCodeInsufficientPermissions_WhenCommentExistsAndUserIsNotOwner()
		{
			User otherUser = new()
			{			
				IsVerified = false,
				UserName = "otheruser",
				Following = [],
				Followers = [],
				RecentSearches = [],
				CreatedAt = DateTime.UtcNow,
				LastLogin = DateTime.UtcNow
			};
			_post.User = otherUser;
			Comment comment = new()
			{
				Content = "Another comment.",
				User = otherUser,
				Post = _post
			};
			await _dbContext.AddAsync(otherUser);
			_post.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			var authorizationServiceMock = new Mock<IAuthorizationService>();
			authorizationServiceMock.Setup(s => s.AuthorizeAsync(_claimsPrincipal, comment, "CanDeleteComment").Result).Returns(AuthorizationResult.Failed);
			CommentService commentService = new(_dbContext, authorizationServiceMock.Object);

			var result = await commentService.DeleteComment(_claimsPrincipal, comment.ID);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.InsufficientPermissions), result.Errors.First().Metadata["code"]);
			Assert.Single(_post.Comments);
		}

		[Fact]
		public async Task DeleteComment_ShouldReturnFailedResultWithErrorCodeNotFound_WhenCommentDoesntExist()
		{
			CommentService commentService = new(_dbContext, null!);

			var result = await commentService.DeleteComment(_claimsPrincipal, Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.NotFound), result.Errors.First().Metadata["code"]);
		}
	}
}
