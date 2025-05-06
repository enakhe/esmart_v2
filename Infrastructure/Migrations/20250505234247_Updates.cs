using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedBy",
                schema: "ESMART",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                schema: "ESMART",
                table: "VerificationCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                schema: "ESMART",
                table: "TransactionItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "ESMART",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
