using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTaskListEntity_And_ListTaskEntity_WithRelationshipOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ListName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskCollections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaskListId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCollections_TaskItems_TaskListId",
                        column: x => x.TaskListId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskCollections_IsDeleted",
                table: "TaskCollections",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCollections_TaskListId",
                table: "TaskCollections",
                column: "TaskListId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_IsDeleted",
                table: "TaskItems",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskCollections");

            migrationBuilder.DropTable(
                name: "TaskItems");
        }
    }
}
