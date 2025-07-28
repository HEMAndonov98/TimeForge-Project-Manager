using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedisRunningtoTimeEntryentityandmadeEndnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "TimeEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "TimeEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "TimeEntries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "TimeEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
