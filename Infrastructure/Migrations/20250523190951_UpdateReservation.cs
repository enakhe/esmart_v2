using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                schema: "ESMART",
                table: "Reservations");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "ESMART",
                table: "TransactionItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                schema: "ESMART",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
