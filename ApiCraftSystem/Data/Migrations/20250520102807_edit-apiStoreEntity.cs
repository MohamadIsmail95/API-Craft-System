using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class editapiStoreEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MethodeType",
                table: "ApiStores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MethodeType",
                table: "ApiStores");
        }
    }
}
