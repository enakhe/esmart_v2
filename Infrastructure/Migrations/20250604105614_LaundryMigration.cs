using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LaundryMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LaundryOrders",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Invoice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoomBookingId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GuestAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaundryOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaundryOrders_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "ESMART",
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LaundryOrders_GuestAccounts_GuestAccountId",
                        column: x => x.GuestAccountId,
                        principalSchema: "ESMART",
                        principalTable: "GuestAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LaundryOrders_RoomBookings_RoomBookingId",
                        column: x => x.RoomBookingId,
                        principalSchema: "ESMART",
                        principalTable: "RoomBookings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LaundaryOrderItems",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LaundryOrderItemId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaundryOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LaundaryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaundryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaundaryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaundaryOrderItems_Laundries_LaundryId",
                        column: x => x.LaundryId,
                        principalSchema: "ESMART",
                        principalTable: "Laundries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LaundaryOrderItems_LaundryOrders_LaundryOrderId",
                        column: x => x.LaundryOrderId,
                        principalSchema: "ESMART",
                        principalTable: "LaundryOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                column: "LaundryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundaryOrderItems_LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundryId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundaryOrderItems_LaundryOrderId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundryOrders_BookingId",
                schema: "ESMART",
                table: "LaundryOrders",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundryOrders_GuestAccountId",
                schema: "ESMART",
                table: "LaundryOrders",
                column: "GuestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundryOrders_RoomBookingId",
                schema: "ESMART",
                table: "LaundryOrders",
                column: "RoomBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_LaundryOrders_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                column: "LaundryOrderId",
                principalSchema: "ESMART",
                principalTable: "LaundryOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_LaundryOrders_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "LaundaryOrderItems",
                schema: "ESMART");

            migrationBuilder.DropTable(
                name: "LaundryOrders",
                schema: "ESMART");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");
        }
    }
}
