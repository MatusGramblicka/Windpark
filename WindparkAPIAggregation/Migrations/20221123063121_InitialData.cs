using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindparkAPIAggregation.Migrations
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WindPark",
                columns: table => new
                {
                    WindParkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WindParkNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindPark", x => x.WindParkId);
                });

            migrationBuilder.CreateTable(
                name: "Turbine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurbineNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentProduction = table.Column<double>(type: "float", nullable: false),
                    WindSpeed = table.Column<double>(type: "float", nullable: false),
                    WindParkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turbine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turbine_WindPark_WindParkId",
                        column: x => x.WindParkId,
                        principalTable: "WindPark",
                        principalColumn: "WindParkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turbine_WindParkId",
                table: "Turbine",
                column: "WindParkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Turbine");

            migrationBuilder.DropTable(
                name: "WindPark");
        }
    }
}
