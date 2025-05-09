using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuthorizationCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorizationCards",
                schema: "ESMART",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ComputerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCards", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationCards",
                schema: "ESMART");
        }
    }
}
