using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class addtenantToApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ApiStores",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiStores_TenantId",
                table: "ApiStores",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiStores_Tenants_TenantId",
                table: "ApiStores",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiStores_Tenants_TenantId",
                table: "ApiStores");

            migrationBuilder.DropIndex(
                name: "IX_ApiStores_TenantId",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ApiStores");
        }
    }
}
