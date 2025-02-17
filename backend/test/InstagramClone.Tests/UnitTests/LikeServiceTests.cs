using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Services;
using InstagramClone.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstagramClone.Tests.UnitTests
{
	public class LikeServiceTests : IDisposable
	{
		private readonly AppDbContext _dbContext;
		private readonly User _user;
		private readonly Post _post;
		private readonly ClaimsPrincipal _claimsPrincipal;
		public LikeServiceTests()
		{
			SqliteConnection sqliteConnection = new SqliteConnection("DataSource=:memory:");
			sqliteConnection.Open();
			DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(sqliteConnection).Options;

			AppDbContext dbContext = new(options);
			dbContext.Database.EnsureCreated();
			User user = new()
			{
				Email = "example@domain.com",
				EmailConfirmed = true,
				UserName = "exampleUsername",
				RealName = "Example Name",
				IsVerified = false,
				CreatedAt = DateTime.Now,
				LastLogin = DateTime.Now,
				Following = [],
				Followers = [],
				RecentSearches = []
			};
			Post post = new() { Photo = "", CreatedAt = DateTime.UtcNow, User = user, Comments = [] };
			dbContext.Add(user);
			dbContext.Add(post);
			dbContext.SaveChanges();

			_dbContext = dbContext;
			_user = user;
			_post = post;

			ClaimsIdentity claimsIdentity = new([new Claim("sub", _user.Id)]);
			ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
			_claimsPrincipal = claimsPrincipal;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		private Comment GetComment() => new()
		{
			Content = "Comment",
			Post = _post,
			User = _user
		};


		[Fact]
		public async Task Like_ShouldReturnSuccess_WhenLikingPost()
		{
			LikeService likeService = new(_dbContext);

			var result = await likeService.Like(_claimsPrincipal, _post);
			await _dbContext.Entry(_post).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Single(_post.Likes);
		}

		[Fact]
		public async Task Like_ShouldReturnSuccess_WhenLikingComment()
		{
			Comment comment = GetComment();
			_post.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			LikeService likeService = new(_dbContext);

			var result = await likeService.Like(_claimsPrincipal, comment);

			Assert.True(result.IsSuccess);
			Assert.Single(comment.Likes);
		}

		[Fact]
		public async Task Like_ShouldReturnFailedResultWithErrorCodeDuplicate_WhenLikingALikedPost()
		{
			LikeService likeService = new(_dbContext);

			await likeService.Like(_claimsPrincipal, _post);
			var result = await likeService.Like(_claimsPrincipal, _post);
			await _dbContext.Entry(_post).ReloadAsync();

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
			Assert.Single(_post.Likes);
		}

		[Fact]
		public async Task Like_ShouldReturnFailedResultWithErrorCodeDuplicate_WhenLikingALikedComment()
		{
			Comment comment = GetComment();
			_post.Comments.Add(comment);
			await _dbContext.SaveChangesAsync();
			LikeService likeService = new(_dbContext);

			await likeService.Like(_claimsPrincipal, comment);
			var result = await likeService.Like(_claimsPrincipal, comment);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
			Assert.Single(comment.Likes);
		}

		[Fact]
		public async Task Unlike_ShouldReturnSuccess_WhenUnlikingALikedPost()
		{
			_post.Likes.Add(_user);
			await _dbContext.SaveChangesAsync();
			LikeService likeService = new(_dbContext);

			var result = await likeService.Unlike(_claimsPrincipal, _post);
			await _dbContext.Entry(_post).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(_post.Likes);
		}

		[Fact]
		public async Task Unlike_ShouldReturnSuccess_WhenUnikingALikedComment()
		{
			Comment comment = GetComment();
			comment.Likes.Add(_user);
			await _dbContext.AddAsync(comment);
			await _dbContext.SaveChangesAsync();
			LikeService likeService = new(_dbContext);

			var result = await likeService.Unlike(_claimsPrincipal, comment);
			await _dbContext.Entry(comment).ReloadAsync();

			Assert.True(result.IsSuccess);
			Assert.Empty(comment.Likes);
		}

		[Fact]
		public async Task Unlike_ShouldReturnFailedResultWithErrorCodeDuplicate_WhenUnlikingANonLikedPost()
		{
			LikeService likeService = new(_dbContext);

			var result = await likeService.Unlike(_claimsPrincipal, _post);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
		}

		[Fact]
		public async Task Unlike_ShouldReturnFailedResultWithErrorCodeDuplicate_WhenUnlikingANonLikedComment()
		{
			Comment comment = GetComment();
			LikeService likeService = new(_dbContext);

			var result = await likeService.Unlike(_claimsPrincipal, comment);

			Assert.False(result.IsSuccess);
			Assert.Equal(Enum.GetName(ErrorCode.Duplicate), result.Errors.First().Metadata["code"]);
		}
	}
}
