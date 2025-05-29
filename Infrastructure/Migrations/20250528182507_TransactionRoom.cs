using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                schema: "ESMART",
                table: "GuestTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestTransactions_RoomId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestTransactions_Rooms_RoomId",
                schema: "ESMART",
                table: "GuestTransactions",
                column: "RoomId",
                principalSchema: "ESMART",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestTransactions_Rooms_RoomId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropIndex(
                name: "IX_GuestTransactions_RoomId",
                schema: "ESMART",
                table: "GuestTransactions");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "ESMART",
                table: "GuestTransactions");
        }
    }
}
