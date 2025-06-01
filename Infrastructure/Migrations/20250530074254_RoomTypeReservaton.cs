using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoomTypeReservaton : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomTypeReservations",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GuestId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoomTypeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdvancePayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BookingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypeReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomTypeReservations_BankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "ESMART",
                        principalTable: "BankAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomTypeReservations_GuestAccounts_GuestAccountId",
                        column: x => x.GuestAccountId,
                        principalSchema: "ESMART",
                        principalTable: "GuestAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomTypeReservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalSchema: "ESMART",
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomTypeReservations_RoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalSchema: "ESMART",
                        principalTable: "RoomTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeReservations_BankAccountId",
                schema: "ESMART",
                table: "RoomTypeReservations",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeReservations_GuestAccountId",
                schema: "ESMART",
                table: "RoomTypeReservations",
                column: "GuestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeReservations_GuestId",
                schema: "ESMART",
                table: "RoomTypeReservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeReservations_RoomTypeId",
                schema: "ESMART",
                table: "RoomTypeReservations",
                column: "RoomTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomTypeReservations",
                schema: "ESMART");
        }
    }
}
