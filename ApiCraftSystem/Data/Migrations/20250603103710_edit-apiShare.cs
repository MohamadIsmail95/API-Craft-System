using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class editapiShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "ApiShares");

            migrationBuilder.AddColumn<string>(
                name: "UserIds",
                table: "ApiShares",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserIds",
                table: "ApiShares");

            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "ApiShares",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
