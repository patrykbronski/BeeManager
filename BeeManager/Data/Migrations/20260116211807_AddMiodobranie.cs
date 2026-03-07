using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeManager.Data.Migrations
{
    public partial class AddMiodobranie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Miodobrania",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UlId = table.Column<int>(type: "int", nullable: false),
                    DataMiodobrania = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypMiodu = table.Column<int>(type: "int", nullable: false),
                    IloscKg = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    Notatki = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Miodobrania", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Miodobrania_Ule_UlId",
                        column: x => x.UlId,
                        principalTable: "Ule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Miodobrania_UlId",
                table: "Miodobrania",
                column: "UlId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Miodobrania");
        }
    }
}
