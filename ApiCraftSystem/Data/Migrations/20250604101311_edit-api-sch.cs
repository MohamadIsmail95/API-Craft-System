using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class editapisch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobPeriodic",
                table: "ApiStores",
                newName: "ScMin");

            migrationBuilder.AddColumn<int>(
                name: "ScHour",
                table: "ApiStores",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScHour",
                table: "ApiStores");

            migrationBuilder.RenameColumn(
                name: "ScMin",
                table: "ApiStores",
                newName: "JobPeriodic");
        }
    }
}
