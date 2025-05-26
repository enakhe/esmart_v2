using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuestSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowBarAndRes",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowLaundry",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowBarAndRes",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "AllowLaundry",
                schema: "ESMART",
                table: "GuestAccounts");
        }
    }
}
