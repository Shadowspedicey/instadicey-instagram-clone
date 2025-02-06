using InstagramClone.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Data
{
	public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
	{
		public DbSet<Post> Posts { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<User>(U =>
			{
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
						j.Property("LikedPostsID").HasColumnName("LikedPostID");
						j.Property("LikesId").HasColumnName("UserID");
					});
				U.HasMany(u => u.SavedPosts)
					.WithMany()
					.UsingEntity(j =>
					{
						j.ToTable("PostsSaves");
						j.Property("SavedPostsID").HasColumnName("SavedPostID");
						j.Property("User1Id").HasColumnName("UserID");
					});
				U.HasMany<Comment>().WithOne(c => c.User);
				U.HasMany<Comment>()
					.WithMany(c => c.Likes)
					.UsingEntity(j =>
					{
						j.ToTable("CommentsLikes");
						j.Property("LikesId").HasColumnName("UserID");
					});
			});
		}
	}
}
