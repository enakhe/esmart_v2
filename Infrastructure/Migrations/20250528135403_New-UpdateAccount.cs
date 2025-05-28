using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdateAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "BankAccountId",
                schema: "ESMART",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BankAccountId",
                schema: "ESMART",
                table: "Bookings",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BankAccount_BankAccountId",
                schema: "ESMART",
                table: "Bookings",
                column: "BankAccountId",
                principalSchema: "ESMART",
                principalTable: "BankAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BankAccount_BankAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BankAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                schema: "ESMART",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
