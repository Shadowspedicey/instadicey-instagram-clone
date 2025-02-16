using FluentResults;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Profile;
using System.Security.Claims;

namespace InstagramClone.Interfaces
{
	public interface IUserService
	{
		public Task<Result<User>> GetUser(string username);
		public Task<Result> EditUserData(ClaimsPrincipal currentUserPrincipal, UserEditDTO userDataDTO, CancellationToken cancellationToken);
		public Task<Result> ChangeUsername(User currentUser, string newUsername);
		public Task<Result> ChangeRealName(User currentUser, string? newRealName);
		public Task<Result> ChangeBio(User currentUser, string? newBio);
		public Task<Result> ChangeProfilePic(User currentUser, IFormFile newProfilePic, CancellationToken cancellationToken);

		public Task<Result> FollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToFollow);
		public Task<Result> UnfollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToUnfollow);

		public Task<Result> SavePost(ClaimsPrincipal currentUserPrincipal, string postID);
		public Task<Result> UnsavePost(ClaimsPrincipal currentUserPrincipal, string postID);

		public Task<Result> AddToRecentSearches(ClaimsPrincipal currentUserPrincipal, string searchedUsername);
		public Task<Result> RemoveFromRecentSearches(ClaimsPrincipal currentUserPrincipal, string removedUsername);
		public Task<Result> ClearRecentSearches(ClaimsPrincipal currentUserPrincipal);
		public Task<Result<IEnumerable<User>>> GetRecentSearches(ClaimsPrincipal currentUserPrincipal);
	}
}
