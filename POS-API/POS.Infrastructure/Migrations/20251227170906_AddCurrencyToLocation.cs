using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CurrencyId",
                table: "Locations",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Currencies_CurrencyId",
                table: "Locations",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Currencies_CurrencyId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CurrencyId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Locations");
        }
    }
}
