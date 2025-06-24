using InstagramClone.Data;
using InstagramClone.Data.Entities;
using InstagramClone.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace InstagramClone.Services
{
	public class GuestBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration) : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		private readonly IConfiguration _configuration = configuration;
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			var success = false;
			while (!success && !cancellationToken.IsCancellationRequested)
			{
				success = await TryCreateGuest();
			}

			await base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var resetInterval = int.TryParse(_configuration["GuestResetIntervalInMinutes"], out int val) ? val : 5;
			while (!stoppingToken.IsCancellationRequested)
			{
				await ResetGuest(stoppingToken);
				await Task.Delay(TimeSpan.FromMinutes(resetInterval), stoppingToken);
			}
		}

		private async Task<bool> TryCreateGuest()
		{
			try
			{
				using IServiceScope scope = _serviceProvider.CreateScope();
				var provider = scope.ServiceProvider;
				AppDbContext dbContext = provider.GetRequiredService<AppDbContext>();
				UserManager<User> userManager = provider.GetRequiredService<UserManager<User>>();
				User? guest = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "guest");
				if (guest is not null)
					return true;

				guest = new()
				{
					UserName = "guest",
					Email = "guest@instadicey.com",
					EmailConfirmed = true,
					IsVerified = true,
					Followers = [],
					Following = [],
					RecentSearches = [],
					CreatedAt = DateTime.UtcNow,
					LastLogin = DateTime.UtcNow,
				};
				var result = await userManager.CreateAsync(guest);
				if (!result.Succeeded) return false;
				return true;
			}
			catch (Exception ex)
			{
				if (ex is DbException && 
					(ex.Message.Contains("no such table") ||
					ex.Message.Contains("relation \"AspNetUsers\" does not exist")))
				{
					using IServiceScope scope = _serviceProvider.CreateScope();
					AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
					await dbContext.Database.MigrateAsync();
				}
				return false;
			}
		}

		private async Task ResetGuest(CancellationToken stoppingToken)
		{
			using IServiceScope scope = _serviceProvider.CreateScope();
			var serviceProvider = scope.ServiceProvider;
			AppDbContext dbContext = serviceProvider.GetRequiredService<AppDbContext>();
			IFileService fileService = serviceProvider.GetRequiredService<IFileService>();

			User guest = await dbContext.Users.FirstAsync(u => u.UserName == "guest", stoppingToken);
			guest.ProfilePic = dbContext.Model.FindEntityType(typeof(User))!.FindProperty(nameof(User.ProfilePic))!.GetDefaultValue()!.ToString();
			guest.RealName = "";
			guest.Bio = "";
			guest.LikedPosts.Clear();
			guest.SavedPosts.Clear();
			guest.RecentSearches.Clear();
			guest.Followers.Clear();
			guest.Following.Clear();
			guest.Posts.Clear();
			var likedComments = await dbContext.Comments.Where(c => c.Likes.Contains(guest)).ToListAsync(stoppingToken);
			likedComments.ForEach(c => c.Likes.Remove(guest));
			await dbContext.SaveChangesAsync(stoppingToken);
			fileService.DeleteFolder(guest.Id);
		}
	}
}
