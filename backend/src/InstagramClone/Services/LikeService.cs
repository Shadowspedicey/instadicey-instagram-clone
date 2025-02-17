using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class LikeService(AppDbContext dbContext) : ILikeService
	{
		private readonly AppDbContext _dbContext = dbContext;
		public Task<Result> Like(ClaimsPrincipal currentUserPrincipal, ILikeable target)
		{
			throw new NotImplementedException();
		}

		public Task<Result> Unlike(ClaimsPrincipal currentUserPrincipal, ILikeable target)
		{
			throw new NotImplementedException();
		}
	}
}
