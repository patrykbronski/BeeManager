using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeManager.Data.Migrations
{
    public partial class Ul_UniqueNumerWPasiece : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ule_PasiekaId",
                table: "Ule");

            migrationBuilder.CreateIndex(
                name: "IX_Ule_PasiekaId_NumerUla",
                table: "Ule",
                columns: new[] { "PasiekaId", "NumerUla" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ule_PasiekaId_NumerUla",
                table: "Ule");

            migrationBuilder.CreateIndex(
                name: "IX_Ule_PasiekaId",
                table: "Ule",
                column: "PasiekaId");
        }
    }
}
