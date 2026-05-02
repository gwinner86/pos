using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sales_CreatedBy",
                table: "Sales",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_CreatedBy",
                table: "Sales",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_CreatedBy",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CreatedBy",
                table: "Sales");
        }
    }
}
