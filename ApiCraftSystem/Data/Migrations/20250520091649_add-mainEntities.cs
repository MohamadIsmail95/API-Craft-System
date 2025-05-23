using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCraftSystem.Migrations
{
    /// <inheritdoc />
    public partial class addmainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerPort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabaseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatabaseUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchemaName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabaseUserPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiStores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiHeaders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeaderKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeaderValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiHeaders_ApiStores_ApiStoreId",
                        column: x => x.ApiStoreId,
                        principalTable: "ApiStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiMaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiMaps_ApiStores_ApiStoreId",
                        column: x => x.ApiStoreId,
                        principalTable: "ApiStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiHeaders_ApiStoreId",
                table: "ApiHeaders",
                column: "ApiStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiMaps_ApiStoreId",
                table: "ApiMaps",
                column: "ApiStoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiHeaders");

            migrationBuilder.DropTable(
                name: "ApiMaps");

            migrationBuilder.DropTable(
                name: "ApiStores");
        }
    }
}
