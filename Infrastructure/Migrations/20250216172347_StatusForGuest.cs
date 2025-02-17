using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StatusForGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "Guests");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Guests",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Guests",
                newName: "FullName");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Guests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
