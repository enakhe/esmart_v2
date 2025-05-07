using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateapplicationRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_ManagerId",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropTable(
                name: "ApplicationRoleCategory",
                schema: "ESMART");

            migrationBuilder.DropTable(
                name: "ApplicationCategory",
                schema: "ESMART");

            migrationBuilder.DropIndex(
                name: "IX_Role_ManagerId",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "NoOfUser",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "ESMART",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Tag",
                schema: "ESMART",
                table: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                schema: "ESMART",
                table: "Role",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<byte[]>(
                name: "Icon",
                schema: "ESMART",
                table: "Role",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                schema: "ESMART",
                table: "Role",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoOfUser",
                schema: "ESMART",
                table: "Role",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                schema: "ESMART",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Tag",
                schema: "ESMART",
                table: "Role",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationCategory",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRoleCategory",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationRoleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRoleCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationRoleCategory_ApplicationCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "ESMART",
                        principalTable: "ApplicationCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationRoleCategory_Role_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalSchema: "ESMART",
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_ManagerId",
                schema: "ESMART",
                table: "Role",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleCategory_ApplicationRoleId",
                schema: "ESMART",
                table: "ApplicationRoleCategory",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRoleCategory_CategoryId",
                schema: "ESMART",
                table: "ApplicationRoleCategory",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_User_ManagerId",
                schema: "ESMART",
                table: "Role",
                column: "ManagerId",
                principalSchema: "ESMART",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
