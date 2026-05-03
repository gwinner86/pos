using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSetupExpenseTypeAndLocationVat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_GLAccounts_ExpenseAccountId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Payee",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "ExpenseAccountId",
                table: "Expenses",
                newName: "ExpenseTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_ExpenseAccountId",
                table: "Expenses",
                newName: "IX_Expenses_ExpenseTypeId");

            migrationBuilder.AddColumn<int>(
                name: "VatCalculationType",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SetupExpenseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpenseAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupExpenseTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupExpenseTypes_GLAccounts_ExpenseAccountId",
                        column: x => x.ExpenseAccountId,
                        principalTable: "GLAccounts",
                        principalColumn: "GLAccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetupExpenseTypes_ExpenseAccountId",
                table: "SetupExpenseTypes",
                column: "ExpenseAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_SetupExpenseTypes_ExpenseTypeId",
                table: "Expenses",
                column: "ExpenseTypeId",
                principalTable: "SetupExpenseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_SetupExpenseTypes_ExpenseTypeId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "SetupExpenseTypes");

            migrationBuilder.DropColumn(
                name: "VatCalculationType",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "ExpenseTypeId",
                table: "Expenses",
                newName: "ExpenseAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Expenses_ExpenseTypeId",
                table: "Expenses",
                newName: "IX_Expenses_ExpenseAccountId");

            migrationBuilder.AddColumn<string>(
                name: "Payee",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_GLAccounts_ExpenseAccountId",
                table: "Expenses",
                column: "ExpenseAccountId",
                principalTable: "GLAccounts",
                principalColumn: "GLAccountId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
