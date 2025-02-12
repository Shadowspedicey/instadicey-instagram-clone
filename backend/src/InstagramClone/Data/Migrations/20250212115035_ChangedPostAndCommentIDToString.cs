using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstagramClone.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPostAndCommentIDToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_PostsSaves_Posts_SavedPostID", table: "PostsSaves");

            migrationBuilder.DropForeignKey("FK_PostsLikes_Posts_LikedPostID", "PostsLikes");

            migrationBuilder.DropForeignKey("FK_Comments_Posts_PostID", "Comments");

            migrationBuilder.DropForeignKey("FK_CommentsLikes_Comments_CommentID", "CommentsLikes");

            migrationBuilder.DropPrimaryKey("PK_PostsSaves", "PostsSaves");

            migrationBuilder.DropPrimaryKey("PK_PostsLikes", "PostsLikes");

            migrationBuilder.DropPrimaryKey("PK_Posts", "Posts");

            migrationBuilder.DropPrimaryKey("PK_CommentsLikes", "CommentsLikes");

            migrationBuilder.DropPrimaryKey("PK_Comments", "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "SavedPostID",
                table: "PostsSaves",
                type: "nvarchar(26)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "LikedPostID",
                table: "PostsLikes",
                type: "nvarchar(26)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                table: "Posts",
                type: "nvarchar(26)",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CommentID",
                table: "CommentsLikes",
                type: "nvarchar(26)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PostID",
                table: "Comments",
                type: "nvarchar(26)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                table: "Comments",
                type: "nvarchar(26)",
                maxLength: 26,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey("PK_PostsSaves", "PostsSaves", ["SavedPostID", "UserID"]);

            migrationBuilder.AddPrimaryKey("PK_PostsLikes", "PostsLikes", ["LikedPostID", "UserID"]);

            migrationBuilder.AddPrimaryKey("PK_Posts", "Posts", "ID");

            migrationBuilder.AddPrimaryKey("PK_CommentsLikes", "CommentsLikes", ["CommentID", "UserID"]);

            migrationBuilder.AddPrimaryKey("PK_Comments", "Comments", "ID");

            migrationBuilder.AddForeignKey("FK_PostsSaves_Posts_SavedPostID", "PostsSaves", "SavedPostID", "Posts", principalColumn: "ID");

            migrationBuilder.AddForeignKey("FK_PostsLikes_Posts_LikedPostID", "PostsLikes", "LikedPostID", "Posts", principalColumn: "ID");

            migrationBuilder.AddForeignKey("FK_Comments_Posts_PostID", "Comments", "PostID", "Posts", principalColumn: "ID");

            migrationBuilder.AddForeignKey("FK_CommentsLikes_Comments_CommentID", "CommentsLikes", "CommentID", "Comments", principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SavedPostID",
                table: "PostsSaves",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)");

            migrationBuilder.AlterColumn<int>(
                name: "LikedPostID",
                table: "PostsLikes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Posts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)",
                oldMaxLength: 26);

            migrationBuilder.AlterColumn<int>(
                name: "CommentID",
                table: "CommentsLikes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)");

            migrationBuilder.AlterColumn<int>(
                name: "PostID",
                table: "Comments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)");

            migrationBuilder.AlterColumn<int>(
                name: "ID",
                table: "Comments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(26)",
                oldMaxLength: 26);
        }
    }
}
