using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class CommentService(AppDbContext dbContext, IAuthorizationService authorizationService) : ICommentService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly IAuthorizationService _authorizationService = authorizationService;

		public async Task<Result> AddComment(ClaimsPrincipal currentUserPrincipal, ICommentable target, string comment)
		{
			User user = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			Comment newComment = new()
			{
				Content = comment,
				Post = (Post)target,
				User = user,
			};

			target.AddComment(newComment);

			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> DeleteComment(ClaimsPrincipal currentUserPrincipal, string commentID)
		{
			User user = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			Comment? deletedComment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.ID == commentID);
			if (deletedComment is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "Comment was not found."));

			var authorizationResult = await _authorizationService.AuthorizeAsync(currentUserPrincipal, deletedComment, "CanDeleteComment");
			if (!authorizationResult.Succeeded)
				return Result.Fail(new CodedError(ErrorCode.InsufficientPermissions, "User is not comment's or post's owner."));

			_dbContext.Comments.Remove(deletedComment);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}
	}
}
