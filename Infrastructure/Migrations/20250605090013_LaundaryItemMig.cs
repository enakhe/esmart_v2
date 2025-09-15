using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LaundaryItemMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_LaundryOrders_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                column: "LaundryOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_LaundryOrders_LaundryOrderId",
                schema: "ESMART",
                table: "OrderItems",
                column: "LaundryOrderId",
                principalSchema: "ESMART",
                principalTable: "LaundryOrders",
                principalColumn: "Id");
        }
    }
}
