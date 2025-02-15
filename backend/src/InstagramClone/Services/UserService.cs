using FluentResults;
using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.DTOs.Profile;
using InstagramClone.Interfaces;
using InstagramClone.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InstagramClone.Services
{
	public class UserService(
		AppDbContext dbContext,
		UserManager<User> userManager,
		IFileService fileService) : IUserService
	{
		private readonly AppDbContext _dbContext = dbContext;
		private readonly UserManager<User> _userManager = userManager;
		private readonly IFileService _fileService = fileService;

		public async Task<Result<User>> GetUser(string username)
		{
			User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);

			if (user is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "User was not found."));
			return Result.Ok(user);
		}

		public async Task<Result> EditUserData(ClaimsPrincipal currentUserPrincipal, UserEditDTO userDataDTO, CancellationToken cancellationToken)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			List<IError> errors = [];

			if (currentUser.UserName != userDataDTO.Username)
				errors.AddRange((await ChangeUsername(currentUser, userDataDTO.Username)).Errors);
			if (currentUser.RealName != userDataDTO.RealName)
				errors.AddRange((await ChangeRealName(currentUser, userDataDTO.RealName)).Errors);
			if (currentUser.Bio != userDataDTO.Bio)
				errors.AddRange((await ChangeBio(currentUser, userDataDTO.Bio)).Errors);
			if (userDataDTO.NewProfilePic is not null)
				errors.AddRange((await ChangeProfilePic(currentUser, userDataDTO.NewProfilePic, cancellationToken)).Errors);

			return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
		}
		public async Task<Result> ChangeUsername(User currentUser, string newUsername)
		{
			// TODO: Move to controller filter
			newUsername = newUsername.ToLower();
			bool isDuplicate = await _dbContext.Users.AnyAsync(u => u.UserName == newUsername);
			if (isDuplicate)
				return Result.Fail(new CodedError(ErrorCode.Duplicate, $"The username: {newUsername} already exists."));

			var result = await _userManager.SetUserNameAsync(currentUser, newUsername);
			if (!result.Succeeded)
				return Result.Fail(result.Errors.Select(e => new Error(e.Description).WithMetadata(new Dictionary<string, object> { { "code", e.Code }, { "description", e.Description } })));

			return Result.Ok();
		}

		public async Task<Result> ChangeRealName(User currentUser, string? newRealName)
		{
			// TODO: Move to filter
			if (string.IsNullOrWhiteSpace(newRealName))
				newRealName = null;
			currentUser.RealName = newRealName;
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> ChangeBio(User currentUser, string? newBio)
		{
			// TODO: Move to filter
			if (string.IsNullOrWhiteSpace(newBio))
				newBio = null;
			currentUser.Bio = newBio;
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> ChangeProfilePic(User currentUser, IFormFile? newProfilePic, CancellationToken cancellationToken)
		{
			string filePath = await _fileService.SaveFile(newProfilePic!, currentUser.Id, $"ProfilePic{Path.GetExtension(newProfilePic!.FileName)}", cancellationToken);
			string encryptedFilePath = Helpers.Encryption.Encrypt(filePath);

			currentUser.ProfilePic = encryptedFilePath;
			return Result.Ok();

		}


		public async Task<Result> AddToRecentSearches(ClaimsPrincipal currentUserPrincipal, string searchedUsername)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? searchedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == searchedUsername);
			if (searchedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, $"User '{searchedUsername}' was not found"));

			currentUser.RecentSearches.Remove(searchedUser);

			currentUser.RecentSearches = currentUser.RecentSearches.Prepend(searchedUser).ToList();
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}
		public async Task<Result> RemoveFromRecentSearches(ClaimsPrincipal currentUserPrincipal, string removedUsername)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? removedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == removedUsername);
			if (removedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, $"User '{removedUsername}' was not found"));

			if (!currentUser.RecentSearches.Remove(removedUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, $"User '{removedUsername}' doesn't exist in recent searches."));
			else
			{
				await _dbContext.SaveChangesAsync();
				return Result.Ok();
			}
		}

		public async Task<Result> ClearRecentSearches(ClaimsPrincipal currentUserPrincipal)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			currentUser.RecentSearches.Clear();
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}


		public async Task<Result> FollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToFollow)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? followedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == usernameToFollow);
			if (followedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "User was not found."));

			if (currentUser.Following.Contains(followedUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "User is already followed."));

			currentUser.Following.Add(followedUser);
			followedUser.Followers.Add(currentUser);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> UnfollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToUnfollow)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? unfollowedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == usernameToUnfollow);
			if (unfollowedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "User was not found."));

			if (!currentUser.Following.Contains(unfollowedUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "User is already not followed."));

			currentUser.Following.Remove(unfollowedUser);
			unfollowedUser.Followers.Remove(currentUser);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> SavePost(ClaimsPrincipal currentUserPrincipal, string postID)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "Post was not found."));

			if (currentUser.SavedPosts.Contains(post))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "Post is already saved."));

			currentUser.SavedPosts.Add(post);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> UnsavePost(ClaimsPrincipal currentUserPrincipal, string postID)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			Post? post = await _dbContext.Posts.FindAsync(postID);
			if (post is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "Post was not found."));

			if (!currentUser.SavedPosts.Contains(post))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "Post is already not saved."));

			currentUser.SavedPosts.Remove(post);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}
	}
}
