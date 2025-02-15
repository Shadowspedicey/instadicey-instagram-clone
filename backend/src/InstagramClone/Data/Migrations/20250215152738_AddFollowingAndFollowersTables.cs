using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstagramClone.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowingAndFollowersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFollowing",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FollowedUserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollowing", x => new { x.UserID, x.FollowedUserID });
                    table.ForeignKey(
                        name: "FK_UserFollowing_AspNetUsers_FollowedUserID",
                        column: x => x.FollowedUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFollowing_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollowing_FollowedUserID",
                table: "UserFollowing",
                column: "FollowedUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFollowing");
        }
    }
}
