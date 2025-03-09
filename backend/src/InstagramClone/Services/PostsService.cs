using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using System.Security.Claims;
using InstagramClone.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InstagramClone.Services
{
	public class PostsService(
		AppDbContext dbContext,
		IFileService fileService,
		IAuthorizationService authorizationService) : IPostsService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IFileService _fileService = fileService;
		private readonly IAuthorizationService _authorizationService = authorizationService;

		public async Task<Result<Post>> CreatePost(ClaimsPrincipal user, PostCreateDTO postDTO, CancellationToken cancellationToken)
		{
			string postID = Ulid.NewUlid().ToString();
			string userID = user.FindFirstValue("sub")!;
			string? filePath = default;
			try
			{
				filePath = await Helpers.Files.SavePost(_fileService, postDTO.Photo, userID, postID, cancellationToken);
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
				return Result.Ok(newPost);
			}
			catch (Exception ex)
			{
				_fileService.DeleteFile(filePath!);
				return Result.Fail(ex.Message);
			}
		}

		public async Task<Result> DeletePost(ClaimsPrincipal user, string postID)
		{
			Post? post = await _dbContext.Posts.Include(p => p.User).FirstOrDefaultAsync(p => p.ID == postID);
			if (post is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "Post was not found."));

			var authorizationResult = await _authorizationService.AuthorizeAsync(user, post, "CanDeletePost");
			if (!authorizationResult.Succeeded)
				return Result.Fail(new CodedError(ErrorCode.InsufficientPermissions, "User doesn't have the permission to delete the post"));

			_dbContext.Posts.Remove(post);
			await _dbContext.SaveChangesAsync();
			_fileService.DeleteFile(Helpers.Encryption.Decrypt(post.Photo));
			return Result.Ok();
		}

		public async Task<Result<Post>> GetPost(string postID)
		{
			Post? post = await _dbContext.Posts
				.Include(p => p.User)
				.Include(p => p.Likes)
				.Include(p => p.Comments)
				.FirstOrDefaultAsync(p => p.ID == postID);
			return post == null ? Result.Fail(new CodedError(ErrorCode.NotFound, "Post was not found.")) : Result.Ok(post);
		}

		public async Task<Result<ICollection<Post>>> GetMorePosts(string postID)
		{
			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "Post was not found."));

			ICollection<Post> recentPosts = [.. post.User.Posts.Where(p => p != post).OrderByDescending(p => p.CreatedAt).Take(3)];
			return Result.Ok(recentPosts);
		}

		public Task<Result<ICollection<Comment>>> GetPostsComments(string postID)
		{
			throw new NotImplementedException();
		}
	}
}
