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

		public async Task<Result<ICollection<User>>> SearchForUsers(string searchTerm)
		{
			var users = (ICollection<User>) await _dbContext.Users.AsNoTracking().Where(u => EF.Functions.Like(u.UserName, $"%{searchTerm}%")).ToListAsync();

			return Result.Ok(users);
		}

		public async Task<Result> EditUserData(ClaimsPrincipal currentUserPrincipal, UserEditDTO userDataDTO)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;

			List<IError> errors = [];

			if (currentUser.UserName != userDataDTO.Username)
				errors.AddRange((await ChangeUsername(currentUser, userDataDTO.Username)).Errors);
			if (currentUser.RealName != userDataDTO.RealName)
				errors.AddRange((await ChangeRealName(currentUser, userDataDTO.RealName)).Errors);
			if (currentUser.Bio != userDataDTO.Bio)
				errors.AddRange((await ChangeBio(currentUser, userDataDTO.Bio)).Errors);

			return errors.Count == 0 ? Result.Ok() : Result.Fail(errors);
		}
		public async Task<Result> ChangeUsername(User currentUser, string newUsername)
		{
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

		public async Task<Result> ChangeProfilePic(ClaimsPrincipal currentUserPrincipal, IFormFile? newProfilePic, CancellationToken cancellationToken)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			string filePath = await _fileService.SaveFile(newProfilePic!, currentUser.Id, $"ProfilePic{Path.GetExtension(newProfilePic!.FileName)}", cancellationToken);
			string encryptedFilePath = Helpers.Encryption.Encrypt(filePath);

			currentUser.ProfilePic = encryptedFilePath;
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result> ResetProfilePic(ClaimsPrincipal currentUserPrincipal)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			currentUser.ProfilePic = _dbContext.Model.FindEntityType(typeof(User))!.FindProperty(nameof(User.ProfilePic))!.GetDefaultValue()!.ToString();
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}


		public async Task<Result> AddToRecentSearches(ClaimsPrincipal currentUserPrincipal, string searchedUsername)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? searchedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == searchedUsername);
			if (searchedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, $"User '{searchedUsername}' was not found"));

			UserSearch? userSearchResult = currentUser.RecentSearches.FirstOrDefault(us => us.SearchedUser.UserName == searchedUsername);

			if (userSearchResult is null)
				currentUser.RecentSearches.Add(new UserSearch { User = currentUser, SearchedUser = searchedUser, SearchedAt = DateTime.Now });
			else userSearchResult.SearchedAt = DateTime.Now;

			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}
		public async Task<Result> RemoveFromRecentSearches(ClaimsPrincipal currentUserPrincipal, string removedUsername)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? removedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == removedUsername);
			if (removedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, $"User '{removedUsername}' was not found"));

			UserSearch? userSearchResult = currentUser.RecentSearches.FirstOrDefault(us => us.SearchedUser.UserName == removedUsername);
			if (userSearchResult is null)
				return Result.Fail(new CodedError(ErrorCode.Duplicate, $"User '{removedUsername}' doesn't exist in recent searches."));
			else
			{
				currentUser.RecentSearches.Remove(userSearchResult);
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

		public async Task<Result<IEnumerable<User>>> GetRecentSearches(ClaimsPrincipal currentUserPrincipal)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			return Result.Ok(currentUser.RecentSearches.OrderByDescending(us => us.SearchedAt).Select(us => us.SearchedUser));
		}


		public async Task<Result> FollowUser(ClaimsPrincipal currentUserPrincipal, string usernameToFollow)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? followedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == usernameToFollow);

			if (currentUser == followedUser)
				return Result.Fail(new CodedError(ErrorCode.InvalidInput, "A user can't follow himself."));

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

			if (currentUser == unfollowedUser)
				return Result.Fail(new CodedError(ErrorCode.InvalidInput, "A user can't follow himself."));

			if (unfollowedUser is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "User was not found."));

			if (!currentUser.Following.Contains(unfollowedUser))
				return Result.Fail(new CodedError(ErrorCode.Duplicate, "User is already not followed."));

			currentUser.Following.Remove(unfollowedUser);
			unfollowedUser.Followers.Remove(currentUser);
			await _dbContext.SaveChangesAsync();
			return Result.Ok();
		}

		public async Task<Result<bool>> FollowingCheck(ClaimsPrincipal currentUserPrincipal, string usernametoCheck)
		{
			User currentUser = (await _dbContext.Users.FindAsync(currentUserPrincipal.FindFirstValue("sub")))!;
			User? userToCheck = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == usernametoCheck);
			if (userToCheck is null)
				return Result.Fail(new CodedError(ErrorCode.NotFound, "User was not found."));

			return Result.Ok(currentUser.Following.Contains(userToCheck));
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

		public async Task<Result<IList<Post>>> GetSavedPosts(ClaimsPrincipal currentUserPrincipal)
		{
			List<string> savedPostsID = await _dbContext.Database.SqlQuery<string>($"SELECT * FROM PostsSaves WHERE UserID = {currentUserPrincipal.FindFirstValue("sub")} ORDER BY SavedAt DESC").ToListAsync();
			IList<Post> savedPosts = await _dbContext.Posts.Where(p => savedPostsID.Contains(p.ID)).ToListAsync();
			var postsDictionary = savedPosts.ToDictionary(p => p.ID);
			IList<Post> savedPostsOrdered = savedPostsID.Select(id => postsDictionary[id]).ToList();
			return Result.Ok(savedPostsOrdered);
		}
	}
}
