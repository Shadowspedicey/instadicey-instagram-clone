using FluentResults;
using InstagramClone.Data.Entities;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface ILikeService
	{
		public Task<Result> Like(ClaimsPrincipal currentUserPrincipal, ILikeable target);
		public Task<Result> Unlike(ClaimsPrincipal currentUserPrincipal, ILikeable target);
	}
}
