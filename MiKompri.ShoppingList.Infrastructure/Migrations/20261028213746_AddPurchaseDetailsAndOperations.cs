using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.ShoppingList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseDetailsAndOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MarketId",
                table: "shared_list_item_expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PurchasedOn",
                table: "shared_list_item_expenses",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "shared_list_item_expenses",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.CreateTable(
                name: "purchase_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ListId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_operations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_operations");

            migrationBuilder.DropColumn(
                name: "MarketId",
                table: "shared_list_item_expenses");

            migrationBuilder.DropColumn(
                name: "PurchasedOn",
                table: "shared_list_item_expenses");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "shared_list_item_expenses");
        }
    }
}
