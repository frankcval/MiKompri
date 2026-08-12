using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiKompri.ShoppingList.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedListsSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "purchase_lists",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AddedBy",
                table: "list_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "list_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "shared_list_audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedPurchaseListId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TargetEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_list_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shared_list_item_expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedListItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaidBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RealPaidPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_list_item_expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shared_list_expense_participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemExpenseRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParticipantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShareAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_list_expense_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shared_list_expense_participants_shared_list_item_expenses_~",
                        column: x => x.ItemExpenseRecordId,
                        principalTable: "shared_list_item_expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shared_list_audit_events_SharedPurchaseListId",
                table: "shared_list_audit_events",
                column: "SharedPurchaseListId");

            migrationBuilder.CreateIndex(
                name: "IX_shared_list_expense_participants_ItemExpenseRecordId_Partic~",
                table: "shared_list_expense_participants",
                columns: new[] { "ItemExpenseRecordId", "ParticipantUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_list_audit_events");

            migrationBuilder.DropTable(
                name: "shared_list_expense_participants");

            migrationBuilder.DropTable(
                name: "shared_list_item_expenses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "purchase_lists");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "list_items");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "list_items");
        }
    }
}
