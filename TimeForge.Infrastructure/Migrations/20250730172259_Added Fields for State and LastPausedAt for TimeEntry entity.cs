using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedFieldsforStateandLastPausedAtforTimeEntryentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "TimeEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPausedAt",
                table: "TimeEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "TimeEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalPausedDuration",
                table: "TimeEntries",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPausedAt",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "State",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "TotalPausedDuration",
                table: "TimeEntries");

            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "TimeEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
