using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Posts;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface IPostsService
	{
		public Task<Result<PostGetDTO>> CreatePost(ClaimsPrincipal user, PostCreateDTO postDTO);
		public Task<Result<PostGetDTO>> GetPost(string postID);
		public Task<Result> DeletePost(ClaimsPrincipal user, string postID);
		public Task<Result<ICollection<Comment>>> GetPostsComments(string postID);

	}
}
