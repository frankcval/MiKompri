using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCanonicalIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdentityProvider_ExternalUserId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalUserId",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "ObjectId",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityProvider_ExternalUserId",
                table: "Users",
                columns: new[] { "IdentityProvider", "ExternalUserId" },
                unique: true,
                filter: "\"ExternalUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_ObjectId",
                table: "Users",
                columns: new[] { "TenantId", "ObjectId" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL AND \"ObjectId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IdentityProvider_ExternalUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_ObjectId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ObjectId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalUserId",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityProvider_ExternalUserId",
                table: "Users",
                columns: new[] { "IdentityProvider", "ExternalUserId" },
                unique: true);
        }
    }
}
