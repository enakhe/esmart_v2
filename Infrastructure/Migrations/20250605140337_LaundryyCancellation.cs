using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LaundryyCancellation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaundaryOrderItems_Laundries_LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_LaundaryOrderItems_LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems");

            migrationBuilder.DropColumn(
                name: "LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaundaryOrderItems_LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LaundaryOrderItems_Laundries_LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundaryId",
                principalSchema: "ESMART",
                principalTable: "Laundries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LaundaryOrderItems_Laundries_LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_LaundaryOrderItems_LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "LaundaryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaundaryOrderItems_LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LaundaryOrderItems_Laundries_LaundryId",
                schema: "ESMART",
                table: "LaundaryOrderItems",
                column: "LaundryId",
                principalSchema: "ESMART",
                principalTable: "Laundries",
                principalColumn: "Id");
        }
    }
}
