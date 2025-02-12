using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using InstagramClone.Interfaces;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class PostsService(AppDbContext dbContext) : IPostsService
	{
		private readonly AppDbContext _dbContext = dbContext;

		public Task<Result> CreatePost(ClaimsPrincipal user, PostCreateDTO postDTO)
		{
			throw new NotImplementedException();
		}

		public Task<Result> DeletePost(ClaimsPrincipal user, string postID)
		{
			throw new NotImplementedException();
		}

		public Task<Result<Post>> GetPost(string postID)
		{
			throw new NotImplementedException();
		}

		public Task<Result<ICollection<Comment>>> GetPostsComments(string postID)
		{
			throw new NotImplementedException();
		}
	}
}
