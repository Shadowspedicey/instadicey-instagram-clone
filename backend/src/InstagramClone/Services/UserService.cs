using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Profile;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class UserService(
		AppDbContext dbContext,
		UserManager<User> userManager,
		IFileService fileService,
		IAuthorizationService authorizationService) : IUserService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly UserManager<User> _userManager = userManager;
		private readonly IFileService _fileService = fileService;
		private readonly IAuthorizationService _authorizationService = authorizationService;

		public Task<Result<User>> GetUser(string username)
		{
			throw new NotImplementedException();
		}

		public Task<Result> EditUserData(ClaimsPrincipal currentUserPrincipal, UserEditDTO userDataDTO)
		{
			throw new NotImplementedException();
		}
		public Task<Result> ChangeUsername(User currentUser, string newUsername)
		{
			throw new NotImplementedException();
		}

		public Task<Result> ChangeRealName(User currentUser, string? newRealName)
		{
			throw new NotImplementedException();
		}

		public Task<Result> ChangeBio(User currentUser, string? newBio)
		{
			throw new NotImplementedException();
		}

		public Task<Result> ChangeProfilePic(User currentUser, IFormFile newProfilePic)
		{
			throw new NotImplementedException();
		}


		public Task<Result> AddToRecentSearches(ClaimsPrincipal currentUserPrincipal, string searchedUsername)
		{
			throw new NotImplementedException();
		}
		public Task<Result> RemoveFromRecentSearches(ClaimsPrincipal currentUserPrincipal, string removedUsername)
		{
			throw new NotImplementedException();
		}

		public Task<Result> ClearRecentSearches(ClaimsPrincipal currentUserPrincipal)
		{
			throw new NotImplementedException();
		}


		public Task<Result> FollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToFollow)
		{
			throw new NotImplementedException();
		}

		public Task<Result> UnfollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToUnfollow)
		{
			throw new NotImplementedException();
		}

		public Task<Result> SavePost(ClaimsPrincipal currentUserPrincipal, string postID)
		{
			throw new NotImplementedException();
		}

		public Task<Result> UnsavePost(ClaimsPrincipal currentUserPrincipal, string postID)
		{
			throw new NotImplementedException();
		}
	}
}
