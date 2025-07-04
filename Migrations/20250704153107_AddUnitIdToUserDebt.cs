using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YoneticiOtomasyonu.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitIdToUserDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "UserDebts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDebts_UnitId",
                table: "UserDebts",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDebts_Units_UnitId",
                table: "UserDebts",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDebts_Units_UnitId",
                table: "UserDebts");

            migrationBuilder.DropIndex(
                name: "IX_UserDebts_UnitId",
                table: "UserDebts");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "UserDebts");
        }
    }
}
