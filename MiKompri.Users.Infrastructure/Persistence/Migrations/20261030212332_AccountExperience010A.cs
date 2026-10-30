using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccountExperience010A : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultListSort",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "newest");

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GivenName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProfileVersion",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SecurityVersion",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TokensValidFromExclusive",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdentityMigrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceTenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetProvider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetTenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetObjectId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApprovalDigest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EvidenceDigest = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    State = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityMigrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityMigrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "\"Username\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMigrations_SourceProvider_SourceTenantId_SourceObje~",
                table: "IdentityMigrations",
                columns: new[] { "SourceProvider", "SourceTenantId", "SourceObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMigrations_TargetProvider_TargetTenantId_TargetObje~",
                table: "IdentityMigrations",
                columns: new[] { "TargetProvider", "TargetTenantId", "TargetObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMigrations_UserId",
                table: "IdentityMigrations",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityMigrations");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultListSort",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GivenName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TokensValidFromExclusive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");
        }
    }
}
