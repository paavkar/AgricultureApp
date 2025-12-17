using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgricultureApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<double>(type: "float", nullable: false),
                    SizeUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SoilType = table.Column<int>(type: "int", nullable: false),
                    FarmId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OwnerFarmId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fields_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Fields_Farms_OwnerFarmId",
                        column: x => x.OwnerFarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FieldCultivations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Crop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedYield = table.Column<double>(type: "float", nullable: true),
                    ActualYield = table.Column<double>(type: "float", nullable: true),
                    YieldUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlantingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HarvestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FarmId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldCultivations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldCultivations_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FieldCultivations_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldCultivations_FarmId",
                table: "FieldCultivations",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldCultivations_FieldId",
                table: "FieldCultivations",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_FarmId",
                table: "Fields",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_OwnerFarmId",
                table: "Fields",
                column: "OwnerFarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldCultivations");

            migrationBuilder.DropTable(
                name: "Fields");
        }
    }
}
