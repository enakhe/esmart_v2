using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GuestAccTransactionUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Balance",
                schema: "ESMART",
                table: "GuestAccounts",
                newName: "TotalCharges");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DirectPayments",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FundedBalance",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TopUps",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_GuestTransactions_ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestTransactions_User_ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "ApplicationUserId",
                principalSchema: "ESMART",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestTransactions_User_ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GuestTransactions_ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "DirectPayments",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "FundedBalance",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "TopUps",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.RenameColumn(
                name: "TotalCharges",
                schema: "ESMART",
                table: "GuestAccounts",
                newName: "Balance");
        }
    }
}
