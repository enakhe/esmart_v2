using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                schema: "ESMART",
                table: "Transactions",
                newName: "TotalRevenue");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalReceivables",
                schema: "ESMART",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                schema: "ESMART",
                table: "TransactionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                schema: "ESMART",
                table: "TransactionItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_User_ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems",
                column: "ApplicationUserId",
                principalSchema: "ESMART",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_User_ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "TotalReceivables",
                schema: "ESMART",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.RenameColumn(
                name: "TotalRevenue",
                schema: "ESMART",
                table: "Transactions",
                newName: "TotalAmount");
        }
    }
}
