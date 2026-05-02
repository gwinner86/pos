using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureSingleCurrencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_CompanyId",
                table: "Currencies");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CompanyId",
                table: "Currencies",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_CompanyId",
                table: "Currencies");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CompanyId",
                table: "Currencies",
                column: "CompanyId");
        }
    }
}
