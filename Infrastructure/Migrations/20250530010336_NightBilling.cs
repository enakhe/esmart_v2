using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NightBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomNightCharges",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomBookingId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Night = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomNightCharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomNightCharges_RoomBookings_RoomBookingId",
                        column: x => x.RoomBookingId,
                        principalSchema: "ESMART",
                        principalTable: "RoomBookings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomNightCharges_RoomBookingId",
                schema: "ESMART",
                table: "RoomNightCharges",
                column: "RoomBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomNightCharges",
                schema: "ESMART");
        }
    }
}
