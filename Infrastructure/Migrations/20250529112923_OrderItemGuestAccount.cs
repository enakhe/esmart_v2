using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderItemGuestAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestAccount",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GuestAccountId",
                schema: "ESMART",
                table: "Orders",
                column: "GuestAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "Orders",
                column: "GuestAccountId",
                principalSchema: "ESMART",
                principalTable: "GuestAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_GuestAccounts_GuestAccountId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_GuestAccountId",
                schema: "ESMART",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "GuestAccountId",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccount",
                schema: "ESMART",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
