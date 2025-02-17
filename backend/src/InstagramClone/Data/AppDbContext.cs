using InstagramClone.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Data
{
	public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
	{
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<UserSearch> UserSearches { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>(U =>
			{
				U.Property(u => u.UserName)
					.HasMaxLength(20)
					.IsRequired();
				U.Property(u => u.IsVerified)
					.HasDefaultValue(false);
				U.Property(u => u.ProfilePic)
					.HasDefaultValue("https://firebasestorage.googleapis.com/v0/b/instadicey.appspot.com/o/default%2FprofilePic.jpg?alt=media&token=3ac835a3-016e-470a-b7b3-f898d82cdbde");
				
				U.HasMany(u => u.Posts)
					.WithOne(p => p.User)
					.OnDelete(DeleteBehavior.Cascade);
				U.HasMany(u => u.LikedPosts)
					.WithMany(p => p.Likes)
					.UsingEntity(j =>
					{
						j.ToTable("PostsLikes");
						j.HasOne(typeof(Post))
							.WithMany()
							.HasForeignKey("LikedPostsID")
							.OnDelete(DeleteBehavior.ClientCascade);
						j.Property("LikesId").HasColumnName("UserID");
						j.Property("LikedPostsID").HasColumnName("LikedPostID");
					});
				U.HasMany(u => u.SavedPosts)
					.WithMany()
					.UsingEntity(j =>
					{
						j.ToTable("PostsSaves");
						j.HasOne(typeof(Post))
							.WithMany()
							.HasForeignKey("SavedPostsID")
							.OnDelete(DeleteBehavior.ClientCascade);
						j.Property("SavedPostsID").HasColumnName("SavedPostID");
						j.Property("User1Id").HasColumnName("UserID");
						j.Property<DateTime>("SavedAt").HasDefaultValueSql(Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer" ? "GETUTCDATE()" : "CURRENT_TIMESTAMP");
					});
				U.HasMany<Comment>()
					.WithMany(c => c.Likes)
					.UsingEntity(j =>
					{
						j.ToTable("CommentsLikes");
						j.Property("LikesId").HasColumnName("UserID");
					});

				U.HasMany(u1 => u1.Following)
					.WithMany(u2 => u2.Followers)
					.UsingEntity(e =>
					{
						e.ToTable("UserFollowing");
						e.Property("FollowersId").HasColumnName("UserID");
						e.Property("FollowingId").HasColumnName("FollowedUserID");
					});
			});

			modelBuilder.Entity<UserSearch>(US =>
			{
				US.HasKey(us => new { us.UserID, us.SearchedUserID });
				US.HasOne(us => us.User)
					.WithMany(u => u.RecentSearches)
					.HasForeignKey(us => us.UserID)
					.OnDelete(DeleteBehavior.ClientCascade);
			});

			modelBuilder.Entity<Post>().Ignore(p => p.SortedComments);

			modelBuilder.Entity<Comment>(C =>
			{
				C.HasOne(c => c.User)
					.WithMany()
					.OnDelete(DeleteBehavior.ClientCascade);
				C.HasOne(c => c.Post)
					.WithMany(p => p.Comments)
					.OnDelete(DeleteBehavior.ClientCascade);
			});
		}
	}
}
