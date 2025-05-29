using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderGuestAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCharge",
                schema: "ESMART",
                table: "RoomBookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "ESMART",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccount",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Invoice",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomBookingId",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCharge",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RoomBookingId",
                schema: "ESMART",
                table: "Orders",
                column: "RoomBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RoomBookings_RoomBookingId",
                schema: "ESMART",
                table: "Orders",
                column: "RoomBookingId",
                principalSchema: "ESMART",
                principalTable: "RoomBookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RoomBookings_RoomBookingId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RoomBookingId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServiceCharge",
                schema: "ESMART",
                table: "RoomBookings");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccount",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Invoice",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RoomBookingId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServiceCharge",
                schema: "ESMART",
                table: "GuestAccounts");
        }
    }
}
