using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                schema: "ESMART",
                table: "User",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                schema: "ESMART",
                table: "User",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_RoleId",
                schema: "ESMART",
                table: "User",
                column: "RoleId",
                principalSchema: "ESMART",
                principalTable: "Role",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_RoleId",
                schema: "ESMART",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_RoleId",
                schema: "ESMART",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "ESMART",
                table: "User");
        }
    }
}
