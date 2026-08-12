using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.ShoppingList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_shared_list_item_expenses_SharedListItemId",
                table: "shared_list_item_expenses",
                column: "SharedListItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_shared_list_item_expenses_list_items_SharedListItemId",
                table: "shared_list_item_expenses",
                column: "SharedListItemId",
                principalTable: "list_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_shared_list_item_expenses_list_items_SharedListItemId",
                table: "shared_list_item_expenses");

            migrationBuilder.DropIndex(
                name: "IX_shared_list_item_expenses_SharedListItemId",
                table: "shared_list_item_expenses");
        }
    }
}
