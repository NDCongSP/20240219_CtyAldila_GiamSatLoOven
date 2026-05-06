using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addEfFT14TipOd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FT14",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateddAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Actived = table.Column<bool>(type: "bit", nullable: true),
                    PartName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreqTarget = table.Column<double>(type: "float", nullable: true),
                    Set_Freq_Offset_Low = table.Column<double>(type: "float", nullable: true),
                    Set_Freq_Offset_Hight = table.Column<double>(type: "float", nullable: true),
                    Formula_F = table.Column<double>(type: "float", nullable: true),
                    A = table.Column<double>(type: "float", nullable: true),
                    B = table.Column<double>(type: "float", nullable: true),
                    C = table.Column<double>(type: "float", nullable: true),
                    D = table.Column<double>(type: "float", nullable: true),
                    TipOdLength_1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diam_LL_1 = table.Column<double>(type: "float", nullable: true),
                    Diam_UL_1 = table.Column<double>(type: "float", nullable: true),
                    TipOdLength_2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diam_LL_2 = table.Column<double>(type: "float", nullable: true),
                    Diam_UL_2 = table.Column<double>(type: "float", nullable: true),
                    TipOdLength_3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diam_LL_3 = table.Column<double>(type: "float", nullable: true),
                    Diam_UL_3 = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT14", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FT14");
        }
    }
}
