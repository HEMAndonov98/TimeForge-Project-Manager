using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimeEntry_IndexAddedforUserIdandState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId_Start",
                table: "TimeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_State",
                table: "TimeEntries",
                columns: new[] { "UserId", "State" },
                unique: true)
                .Annotation("SqlServer:Include", new[] { "TaskId", "Start" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_UserId_State",
                table: "TimeEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_UserId_Start",
                table: "TimeEntries",
                columns: new[] { "UserId", "Start" });
        }
    }
}
