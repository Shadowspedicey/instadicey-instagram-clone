using FluentResults;
using InstagramClone.Data.Entities;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface ICommentService
	{
		public Task<Result> AddComment(ClaimsPrincipal currentUserPrincipal, ICommentable target, string comment);
		public Task<Result> DeleteComment(ClaimsPrincipal currentUserPrincipal, string commentID);
	}
}
