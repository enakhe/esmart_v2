using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InventoryItemID",
                schema: "ESMART",
                table: "MenuItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDirectStock",
                schema: "ESMART",
                table: "MenuItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_InventoryItemID",
                schema: "ESMART",
                table: "MenuItems",
                column: "InventoryItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_InventoryItems_InventoryItemID",
                schema: "ESMART",
                table: "MenuItems",
                column: "InventoryItemID",
                principalSchema: "ESMART",
                principalTable: "InventoryItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_InventoryItems_InventoryItemID",
                schema: "ESMART",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_InventoryItemID",
                schema: "ESMART",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "InventoryItemID",
                schema: "ESMART",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "IsDirectStock",
                schema: "ESMART",
                table: "MenuItems");
        }
    }
}
