using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class LikeService(AppDbContext dbContext) : ILikeService
	{
		private readonly AppDbContext _dbContext = dbContext;
		public async Task<Result> Like(ClaimsPrincipal currentUserPrincipal, ILikeable target)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			if (target.Likes.Contains(currentUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "Post is already liked by user."));

			target.Like(currentUser);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> Unlike(ClaimsPrincipal currentUserPrincipal, ILikeable target)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			if (!target.Likes.Contains(currentUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "Post is already not liked by user."));

			target.Unlike(currentUser);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}
	}
}
