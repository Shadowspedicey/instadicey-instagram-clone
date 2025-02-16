using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class CommentService(AppDbContext dbContext, IAuthorizationService authorizationService) : ICommentService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IAuthorizationService _authorizationService = authorizationService;

		public Task<Result> AddComment(ClaimsPrincipal currentUserPrincipal, ICommentable target, string comment)
		{
			throw new NotImplementedException();
		}

		public Task<Result> DeleteComment(ClaimsPrincipal currentUserPrincipal, string commentID)
		{
			throw new NotImplementedException();
		}
	}
}
