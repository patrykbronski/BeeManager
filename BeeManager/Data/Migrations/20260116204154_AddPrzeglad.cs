using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeManager.Data.Migrations
{
    public partial class AddPrzeglad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Przeglady",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UlId = table.Column<int>(type: "int", nullable: false),
                    DataPrzegladu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StanRodziny = table.Column<int>(type: "int", nullable: false),
                    ObecnoscMatki = table.Column<bool>(type: "bit", nullable: false),
                    IloscCzerwiu = table.Column<int>(type: "int", nullable: false),
                    Notatki = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Przeglady", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Przeglady_Ule_UlId",
                        column: x => x.UlId,
                        principalTable: "Ule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Przeglady_UlId",
                table: "Przeglady",
                column: "UlId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Przeglady");
        }
    }
}
