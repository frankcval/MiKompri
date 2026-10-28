using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.ShoppingList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKnownPriceState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPriceKnown",
                table: "list_items",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPriceKnown",
                table: "list_items");
        }
    }
}
