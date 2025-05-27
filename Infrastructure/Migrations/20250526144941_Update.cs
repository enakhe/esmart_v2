using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingId",
                schema: "ESMART",
                table: "TransactionItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                schema: "ESMART",
                table: "TransactionItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_BookingId",
                schema: "ESMART",
                table: "TransactionItems",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_Bookings_BookingId",
                schema: "ESMART",
                table: "TransactionItems",
                column: "BookingId",
                principalSchema: "ESMART",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_Bookings_BookingId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_BookingId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "BookingId",
                schema: "ESMART",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                schema: "ESMART",
                table: "TransactionItems");
        }
    }
}
