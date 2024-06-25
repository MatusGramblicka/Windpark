using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindParkAPIAggregation.Migrations
{
    /// <inheritdoc />
    public partial class SupportPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportPerson",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TurbineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportPerson_Turbine_TurbineId",
                        column: x => x.TurbineId,
                        principalTable: "Turbine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportPerson_TurbineId",
                table: "SupportPerson",
                column: "TurbineId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportPerson");
        }
    }
}
