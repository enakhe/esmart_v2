using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuestAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuestAccounts_GuestId",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_GuestAccounts_GuestId",
                schema: "ESMART",
                table: "GuestAccounts",
                column: "GuestId",
                unique: true,
                filter: "[GuestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuestAccounts_GuestId",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_GuestAccounts_GuestId",
                schema: "ESMART",
                table: "GuestAccounts",
                column: "GuestId");
        }
    }
}
