using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateapplicationUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_User_ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "DateAssigned",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "ESMART",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "isActive",
                schema: "ESMART",
                table: "UserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAssigned",
                schema: "ESMART",
                table: "UserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "ESMART",
                table: "UserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Id",
                schema: "ESMART",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "ESMART",
                table: "UserRoles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                schema: "ESMART",
                table: "UserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_User_ApplicationUserId",
                schema: "ESMART",
                table: "UserRoles",
                column: "ApplicationUserId",
                principalSchema: "ESMART",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
