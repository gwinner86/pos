using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTenantIdFromFeatureDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureDetails_Tenants_TenantId",
                table: "FeatureDetails");

            migrationBuilder.DropIndex(
                name: "IX_FeatureDetails_TenantId",
                table: "FeatureDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FeatureDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FeatureDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FeatureDetails_TenantId",
                table: "FeatureDetails",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureDetails_Tenants_TenantId",
                table: "FeatureDetails",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
