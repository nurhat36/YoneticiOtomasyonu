using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoneticiOtomasyonu.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementImages1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementImage_Announcements_AnnouncementId",
                table: "AnnouncementImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementImage",
                table: "AnnouncementImage");

            migrationBuilder.RenameTable(
                name: "AnnouncementImage",
                newName: "AnnouncementImages");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementImage_AnnouncementId",
                table: "AnnouncementImages",
                newName: "IX_AnnouncementImages_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementImages",
                table: "AnnouncementImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementImages_Announcements_AnnouncementId",
                table: "AnnouncementImages",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnouncementImages_Announcements_AnnouncementId",
                table: "AnnouncementImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AnnouncementImages",
                table: "AnnouncementImages");

            migrationBuilder.RenameTable(
                name: "AnnouncementImages",
                newName: "AnnouncementImage");

            migrationBuilder.RenameIndex(
                name: "IX_AnnouncementImages_AnnouncementId",
                table: "AnnouncementImage",
                newName: "IX_AnnouncementImage_AnnouncementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AnnouncementImage",
                table: "AnnouncementImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnouncementImage_Announcements_AnnouncementId",
                table: "AnnouncementImage",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
