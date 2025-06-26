using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoneticiOtomasyonu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBuildingRole_Buildings_BuildingId",
                table: "UserBuildingRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBuildingRole_UserProfiles_UserProfileId",
                table: "UserBuildingRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBuildingRole",
                table: "UserBuildingRole");

            migrationBuilder.RenameTable(
                name: "UserBuildingRole",
                newName: "UserBuildingRoles");

            migrationBuilder.RenameIndex(
                name: "IX_UserBuildingRole_UserProfileId",
                table: "UserBuildingRoles",
                newName: "IX_UserBuildingRoles_UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_UserBuildingRole_BuildingId",
                table: "UserBuildingRoles",
                newName: "IX_UserBuildingRoles_BuildingId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Buildings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Buildings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Buildings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserBuildingRoles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "UserBuildingRoles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBuildingRoles",
                table: "UserBuildingRoles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CreatorUserId",
                table: "Buildings",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBuildingRoles_AssignedByUserId",
                table: "UserBuildingRoles",
                column: "AssignedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Users_CreatorUserId",
                table: "Buildings",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuildingRoles_Buildings_BuildingId",
                table: "UserBuildingRoles",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuildingRoles_UserProfiles_UserProfileId",
                table: "UserBuildingRoles",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuildingRoles_Users_AssignedByUserId",
                table: "UserBuildingRoles",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Users_CreatorUserId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBuildingRoles_Buildings_BuildingId",
                table: "UserBuildingRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBuildingRoles_UserProfiles_UserProfileId",
                table: "UserBuildingRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserBuildingRoles_Users_AssignedByUserId",
                table: "UserBuildingRoles");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_CreatorUserId",
                table: "Buildings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBuildingRoles",
                table: "UserBuildingRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserBuildingRoles_AssignedByUserId",
                table: "UserBuildingRoles");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Buildings");

            migrationBuilder.RenameTable(
                name: "UserBuildingRoles",
                newName: "UserBuildingRole");

            migrationBuilder.RenameIndex(
                name: "IX_UserBuildingRoles_UserProfileId",
                table: "UserBuildingRole",
                newName: "IX_UserBuildingRole_UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_UserBuildingRoles_BuildingId",
                table: "UserBuildingRole",
                newName: "IX_UserBuildingRole_BuildingId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "UserBuildingRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "UserBuildingRole",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBuildingRole",
                table: "UserBuildingRole",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuildingRole_Buildings_BuildingId",
                table: "UserBuildingRole",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserBuildingRole_UserProfiles_UserProfileId",
                table: "UserBuildingRole",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
