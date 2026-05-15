using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeManager.Migrations
{
    public partial class AddReviewNoteToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "AspNetUsers");
        }
    }
}
