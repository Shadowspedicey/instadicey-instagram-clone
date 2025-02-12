using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using InstagramClone.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace InstagramClone.Tests.UnitTests
{
	public class PostsServiceTests : IDisposable
	{
		readonly IPostsService _postsService;
		readonly AppDbContext _context;
		readonly ClaimsPrincipal _claimsPrincipal;
		readonly User _user;
		public PostsServiceTests()
		{
			SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();
			var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

			AppDbContext context = new(options);
			context.Database.EnsureCreated();
			User user = new User
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
			context.Add(user);
			context.SaveChanges();

			_postsService = new PostsService(context);
			_context = context;
			_user = user;

			var claims = new List<Claim>() { new Claim("sub", user.Id) };
			var claimsIdentity = new ClaimsIdentity(claims);
			_claimsPrincipal = new(claimsIdentity);
		}
		public void Dispose()
		{
			_context.Dispose();
			GC.SuppressFinalize(this);
		}

		[Fact]
		public async Task CreatePost_ShouldReturnResultOk_WhenValidData()
		{
			byte[] fakeImg = { 255, 216, 255, 224, 0, 16 };
			var stream = new MemoryStream(fakeImg);
			var mockPhoto = new Mock<IFormFile>();
			mockPhoto.Setup(f => f.OpenReadStream()).Returns(stream);
			mockPhoto.Setup(f => f.FileName).Returns("file.jpg");
			mockPhoto.Setup(f => f.Length).Returns(1_000_000); // 1 MB
			PostCreateDTO newPost = new()
			{
				Caption = "",
				Photo = mockPhoto.Object
			};

			var result = await _postsService.CreatePost(_claimsPrincipal, newPost);

			Assert.True(result.IsSuccess);
		}

		[Fact]
		public async Task CreatePost_ShouldReturnResultFailed_WhenInvalidData()
		{
			var mockPhoto = new Mock<IFormFile>();
			mockPhoto.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
			mockPhoto.Setup(f => f.FileName).Returns("file.jpg");
			mockPhoto.Setup(f => f.Length).Returns(100_000_000); // 100 MB
			PostCreateDTO newPost = new()
			{
				Caption = "",
				Photo = mockPhoto.Object
			};

			var result = await _postsService.CreatePost(_claimsPrincipal, newPost);

			Assert.False(result.IsSuccess);
		}

		[Fact]
		public async Task GetPost_ShouldReturnPost_WhenPostExists()
		{
			Post post = new()
			{
				Caption = "Example Caption",
				Photo = "https://www.example.com/photo-url",
				User = null,
				CreatedAt = DateTime.Now,
			};
			await _context.Posts.AddAsync(post);
			await _context.SaveChangesAsync();

			var result = await _postsService.GetPost(post.ID);

			Assert.NotNull(result.Value);
			Assert.Equal(post.ID, result.Value.ID);
		}

		[Fact]
		public async Task GetPost_ShouldReturnFailedResult_WhenPostDoesntExists()
		{
			var result = await _postsService.GetPost(Ulid.NewUlid().ToString());

			Assert.False(result.IsSuccess);
			Assert.Null(result.ValueOrDefault);
		}

		[Fact]
		public async Task DeletePost_ShouldReturnOk_WhenPostExistsAndUserIsThePostsOwner()
		{
			var ownerUser = _user;
			Post post = new()
			{
				Caption = "Example Caption",
				Photo = null,
				User = ownerUser,
				CreatedAt= DateTime.Now,
			};
			await _context.Posts.AddAsync(post);
			await _context.SaveChangesAsync();

			var result = await _postsService.DeletePost(_claimsPrincipal, post.ID);
			Post? postSearchResult = await _context.Posts.FindAsync(post.ID);

			Assert.True(result.IsSuccess);
			Assert.Null(postSearchResult);
		}

		[Fact]
		public async Task DeletePost_ShouldReturnFailedResult_WhenPostExistsAndUserIsNotPostsOwner()
		{
			var ownerUser = new User
			{
				Email = "example2@domain.com",
				UserName = "exampleUsername2",
				RealName = "Example Name",
				IsVerified = false,
				CreatedAt = DateTime.Now,
				LastLogin = DateTime.Now,
				Followers = [],
				Following = [],
				RecentSearches = []
			};
			await _context.Users.AddAsync(ownerUser);
			var post = new Post
			{
				Caption = "Example Caption",
				Photo = null,
				User = ownerUser,
				CreatedAt = DateTime.Now,
			};
			await _context.Posts.AddAsync(post);
			await _context.SaveChangesAsync();

			var result = await _postsService.DeletePost(_claimsPrincipal, post.ID);
			Post? postSearchResult = await _context.Posts.FindAsync(post.ID);

			Assert.False(result.IsSuccess);
			Assert.NotNull(postSearchResult);
		}
	}
}
