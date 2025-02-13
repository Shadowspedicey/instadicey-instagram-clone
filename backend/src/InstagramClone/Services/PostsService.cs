using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using System.Security.Claims;
using InstagramClone.Utils;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Services
{
	public class PostsService(AppDbContext dbContext, IFileService fileService, IHttpContextAccessor httpContextAccessor) : IPostsService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IFileService _fileService = fileService;
		private readonly string downloadFileEndpoint = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}/file/";

		public async Task<Result<PostGetDTO>> CreatePost(ClaimsPrincipal user, PostCreateDTO postDTO)
		{
			string postID = Ulid.NewUlid().ToString();
			string userID = user.FindFirstValue("sub")!;
			string filePath = await Helpers.Files.SavePost(_fileService, postDTO.Photo, userID, postID);
			var encryptedFilePath = Helpers.Encryption.Encrypt(filePath);

			Post newPost = new()
			{
				Caption = postDTO.Caption,
				Photo = encryptedFilePath,
				User = (await _dbContext.Users.FindAsync(userID))!,
				CreatedAt = DateTime.Now,
			};
			await _dbContext.Posts.AddAsync(newPost);
			await _dbContext.SaveChangesAsync();

			return Result.Ok(newPost.GetDTO(downloadFileEndpoint));
		}

		public Task<Result> DeletePost(ClaimsPrincipal user, string postID)
		{
			throw new NotImplementedException();
		}

		public async Task<Result<PostGetDTO>> GetPost(string postID)
		{
			Post? post = await _dbContext.Posts
				.Include(p => p.User)
				.Include(p => p.Likes)
				.Include(p => p.Comments)
				.FirstOrDefaultAsync(p => p.ID == postID);
			return post == null ? Result.Fail("Not found.") : Result.Ok(post.GetDTO(downloadFileEndpoint));
		}

		public Task<Result<ICollection<Comment>>> GetPostsComments(string postID)
		{
			throw new NotImplementedException();
		}
	}
}
