using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESMART.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ESMART");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "Identity",
                newName: "UserTokens",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "Identity",
                newName: "UserRoles",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                schema: "Identity",
                newName: "UserLogins",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                schema: "Identity",
                newName: "UserClaims",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "User",
                schema: "Identity",
                newName: "User",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "Identity",
                newName: "Transactions",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "TransactionItems",
                schema: "Identity",
                newName: "TransactionItems",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                schema: "Identity",
                newName: "RoleClaims",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "Role",
                schema: "Identity",
                newName: "Role",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "Guests",
                schema: "Identity",
                newName: "Guests",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "GuestIdentities",
                schema: "Identity",
                newName: "GuestIdentities",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "ApplicationRoleCategory",
                schema: "Identity",
                newName: "ApplicationRoleCategory",
                newSchema: "ESMART");

            migrationBuilder.RenameTable(
                name: "ApplicationCategory",
                schema: "Identity",
                newName: "ApplicationCategory",
                newSchema: "ESMART");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "ESMART",
                newName: "UserTokens",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "ESMART",
                newName: "UserRoles",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                schema: "ESMART",
                newName: "UserLogins",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                schema: "ESMART",
                newName: "UserClaims",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "User",
                schema: "ESMART",
                newName: "User",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "ESMART",
                newName: "Transactions",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "TransactionItems",
                schema: "ESMART",
                newName: "TransactionItems",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                schema: "ESMART",
                newName: "RoleClaims",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "Role",
                schema: "ESMART",
                newName: "Role",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "Guests",
                schema: "ESMART",
                newName: "Guests",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "GuestIdentities",
                schema: "ESMART",
                newName: "GuestIdentities",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "ApplicationRoleCategory",
                schema: "ESMART",
                newName: "ApplicationRoleCategory",
                newSchema: "Identity");

            migrationBuilder.RenameTable(
                name: "ApplicationCategory",
                schema: "ESMART",
                newName: "ApplicationCategory",
                newSchema: "Identity");
        }
    }
}
