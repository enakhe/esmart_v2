using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoomSeetingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "ESMART",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoomTypeId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuildingId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AreaId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloorId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_AreaId",
                schema: "ESMART",
                table: "Rooms",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BuildingId",
                schema: "ESMART",
                table: "Rooms",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FloorId",
                schema: "ESMART",
                table: "Rooms",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomTypeId",
                schema: "ESMART",
                table: "Rooms",
                column: "RoomTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Areas_AreaId",
                schema: "ESMART",
                table: "Rooms",
                column: "AreaId",
                principalSchema: "ESMART",
                principalTable: "Areas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Buildings_BuildingId",
                schema: "ESMART",
                table: "Rooms",
                column: "BuildingId",
                principalSchema: "ESMART",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Floors_FloorId",
                schema: "ESMART",
                table: "Rooms",
                column: "FloorId",
                principalSchema: "ESMART",
                principalTable: "Floors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeId",
                schema: "ESMART",
                table: "Rooms",
                column: "RoomTypeId",
                principalSchema: "ESMART",
                principalTable: "RoomTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Areas_AreaId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Buildings_BuildingId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Floors_FloorId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomTypes_RoomTypeId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_AreaId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_BuildingId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_FloorId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_RoomTypeId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "FloorId",
                schema: "ESMART",
                table: "Rooms");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "ESMART",
                table: "Rooms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RoomTypeId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuildingId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AreaId",
                schema: "ESMART",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
