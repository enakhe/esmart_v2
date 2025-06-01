using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoomFolioUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingId",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestTransactions_BookingId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestTransactions_Bookings_BookingId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "BookingId",
                principalSchema: "ESMART",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestTransactions_Bookings_BookingId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GuestTransactions_BookingId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "BookingId",
                schema: "ESMART",
                table: "GuestTransactions");
        }
    }
}
