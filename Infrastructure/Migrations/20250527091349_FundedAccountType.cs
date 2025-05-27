using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FundedAccountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GuestTransactions_BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestTransactions_BankAccount_BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "BankAccountId",
                principalSchema: "ESMART",
                principalTable: "BankAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestTransactions_BankAccount_BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GuestTransactions_BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "ESMART",
                table: "GuestTransactions");
        }
    }
}
