using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoneticiOtomasyonu.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugAndLastActiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActiveAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Users");
        }
    }
}
