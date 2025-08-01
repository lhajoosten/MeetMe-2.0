using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetMe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_IsActive",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_Title",
                table: "Meetings",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Meetings_Title",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_IsActive",
                table: "Meetings",
                column: "IsActive");
        }
    }
}
