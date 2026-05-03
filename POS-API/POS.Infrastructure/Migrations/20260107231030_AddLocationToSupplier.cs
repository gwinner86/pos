using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_LocationId",
                table: "Suppliers",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Locations_LocationId",
                table: "Suppliers",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Locations_LocationId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_LocationId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Suppliers");
        }
    }
}
