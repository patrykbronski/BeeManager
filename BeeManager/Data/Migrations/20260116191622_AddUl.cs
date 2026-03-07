using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeManager.Data.Migrations
{
    public partial class AddUl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PasiekaId = table.Column<int>(type: "int", nullable: false),
                    NumerUla = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypUla = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DataZalozenia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ule_Pasieki_PasiekaId",
                        column: x => x.PasiekaId,
                        principalTable: "Pasieki",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ule_PasiekaId",
                table: "Ule",
                column: "PasiekaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ule");
        }
    }
}
