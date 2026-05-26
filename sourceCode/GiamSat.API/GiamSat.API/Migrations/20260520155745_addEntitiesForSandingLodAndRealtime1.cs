using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addEntitiesForSandingLodAndRealtime1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FT015",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C001 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT015", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT016",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Formula = table.Column<int>(type: "int", nullable: true),
                    LogType = table.Column<int>(type: "int", nullable: true),
                    ShaftNum = table.Column<int>(type: "int", nullable: true),
                    MotorSandingSpeed = table.Column<double>(type: "float", nullable: true),
                    SpineA = table.Column<double>(type: "float", nullable: true),
                    SpineB = table.Column<double>(type: "float", nullable: true),
                    SpineTarget = table.Column<double>(type: "float", nullable: true),
                    Spine_Low = table.Column<double>(type: "float", nullable: true),
                    Spine_Hight = table.Column<double>(type: "float", nullable: true),
                    OK_NG_SpineB = table.Column<int>(type: "int", nullable: true),
                    TipOdLength_1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipOdLength_2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipOdLength_3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diam_Reading_1 = table.Column<double>(type: "float", nullable: true),
                    Diam_Reading_2 = table.Column<double>(type: "float", nullable: true),
                    Diam_Reading_3 = table.Column<double>(type: "float", nullable: true),
                    OK_NG_OD_1 = table.Column<int>(type: "int", nullable: true),
                    OK_NG_OD_2 = table.Column<int>(type: "int", nullable: true),
                    OK_NG_OD_3 = table.Column<int>(type: "int", nullable: true),
                    Diam_LL_1 = table.Column<int>(type: "int", nullable: true),
                    Diam_LL_2 = table.Column<int>(type: "int", nullable: true),
                    Diam_LL_3 = table.Column<int>(type: "int", nullable: true),
                    Diam_UL_1 = table.Column<int>(type: "int", nullable: true),
                    Diam_UL_2 = table.Column<int>(type: "int", nullable: true),
                    Diam_UL_3 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT016", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FT015");

            migrationBuilder.DropTable(
                name: "FT016");
        }
    }
}
