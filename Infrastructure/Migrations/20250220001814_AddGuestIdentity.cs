using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "IdType",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "IdentificationDocumentBack",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "IdentificationDocumentFront",
                table: "Guests");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Guests",
                newName: "UpdatedBy");

            migrationBuilder.CreateTable(
                name: "GuestIdentities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentificationDocumentFront = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IdentificationDocumentBack = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GuestId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestIdentities_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestIdentities_GuestId",
                table: "GuestIdentities",
                column: "GuestId",
                unique: true,
                filter: "[GuestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestIdentities");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Guests",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "Guests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdType",
                table: "Guests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "IdentificationDocumentBack",
                table: "Guests",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "IdentificationDocumentFront",
                table: "Guests",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
