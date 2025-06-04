using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class editapiStorenewProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthHeaderParam",
                table: "ApiStores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuthMethodeType",
                table: "ApiStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthResponseParam",
                table: "ApiStores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthUrl",
                table: "ApiStores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthUrlBody",
                table: "ApiStores",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthHeaderParam",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "AuthMethodeType",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "AuthResponseParam",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "AuthUrl",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "AuthUrlBody",
                table: "ApiStores");
        }
    }
}
