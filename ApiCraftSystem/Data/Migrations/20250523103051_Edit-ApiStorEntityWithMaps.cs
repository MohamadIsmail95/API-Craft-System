using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class EditApiStorEntityWithMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServerUserName",
                table: "ApiStores");

            migrationBuilder.AddColumn<int>(
                name: "DatabaseType",
                table: "ApiStores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "ApiMaps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatabaseType",
                table: "ApiStores");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "ApiMaps");

            migrationBuilder.AddColumn<string>(
                name: "ServerUserName",
                table: "ApiStores",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
