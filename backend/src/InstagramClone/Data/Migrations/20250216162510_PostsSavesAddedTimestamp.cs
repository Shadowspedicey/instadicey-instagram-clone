using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstagramClone.Data.Migrations
{
    /// <inheritdoc />
    public partial class PostsSavesAddedTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SavedAt",
                table: "PostsSaves",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedAt",
                table: "PostsSaves");
        }
    }
}
