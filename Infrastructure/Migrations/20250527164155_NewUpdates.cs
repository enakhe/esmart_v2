using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentificationDocumentBack",
                schema: "ESMART",
                table: "GuestIdentities");

            migrationBuilder.DropColumn(
                name: "Receivables",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "IdentificationDocumentFront",
                schema: "ESMART",
                table: "GuestIdentities",
                newName: "Document");

            migrationBuilder.RenameColumn(
                name: "TotalCharges",
                schema: "ESMART",
                table: "GuestAccounts",
                newName: "Tax");

            migrationBuilder.AddColumn<decimal>(
                name: "BillPosts",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Invoice",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Payment",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Invoice",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherCharges",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Refunds",
                schema: "ESMART",
                table: "GuestAccounts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingDetail",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GuestAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InvoiceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingDetail_GuestAccounts_GuestAccountId",
                        column: x => x.GuestAccountId,
                        principalSchema: "ESMART",
                        principalTable: "GuestAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingDetail_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "ESMART",
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoomBookings",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookingId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OccupantName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomBookings_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "ESMART",
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomBookings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "ESMART",
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestTransactions_GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "GuestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestAccountId",
                schema: "ESMART",
                table: "Bookings",
                column: "GuestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetail_GuestAccountId",
                schema: "ESMART",
                table: "BookingDetail",
                column: "GuestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetail_RoomId",
                schema: "ESMART",
                table: "BookingDetail",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBookings_BookingId",
                schema: "ESMART",
                table: "RoomBookings",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBookings_RoomId",
                schema: "ESMART",
                table: "RoomBookings",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "Bookings",
                column: "GuestAccountId",
                principalSchema: "ESMART",
                principalTable: "GuestAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestTransactions_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "GuestAccountId",
                principalSchema: "ESMART",
                principalTable: "GuestAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestTransactions_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropTable(
                name: "BookingDetail",
                schema: "ESMART");

            migrationBuilder.DropTable(
                name: "RoomBookings",
                schema: "ESMART");

            migrationBuilder.DropIndex(
                name: "IX_GuestTransactions_GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_GuestAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BillPosts",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "Discount",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "Invoice",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "Payment",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "Tax",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "Discount",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "Invoice",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "OtherCharges",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "Refunds",
                schema: "ESMART",
                table: "GuestAccounts");

            migrationBuilder.DropColumn(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "Document",
                schema: "ESMART",
                table: "GuestIdentities",
                newName: "IdentificationDocumentFront");

            migrationBuilder.RenameColumn(
                name: "Tax",
                schema: "ESMART",
                table: "GuestAccounts",
                newName: "TotalCharges");

            migrationBuilder.AddColumn<byte[]>(
                name: "IdentificationDocumentBack",
                schema: "ESMART",
                table: "GuestIdentities",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Receivables",
                schema: "ESMART",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                schema: "ESMART",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
