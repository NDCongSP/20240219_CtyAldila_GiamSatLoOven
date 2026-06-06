using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class updateEntityFT14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Set_Freq_Offset_Low",
                table: "FT14",
                newName: "Z_Stiffness");

            migrationBuilder.RenameColumn(
                name: "Set_Freq_Offset_Hight",
                table: "FT14",
                newName: "OD_BOD");

            migrationBuilder.RenameColumn(
                name: "Formula_F",
                table: "FT14",
                newName: "FreqTargetLow");

            migrationBuilder.AddColumn<double>(
                name: "Formula",
                table: "FT14",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FreqTargetHight",
                table: "FT14",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Formula",
                table: "FT14");

            migrationBuilder.DropColumn(
                name: "FreqTargetHight",
                table: "FT14");

            migrationBuilder.RenameColumn(
                name: "Z_Stiffness",
                table: "FT14",
                newName: "Set_Freq_Offset_Low");

            migrationBuilder.RenameColumn(
                name: "OD_BOD",
                table: "FT14",
                newName: "Set_Freq_Offset_Hight");

            migrationBuilder.RenameColumn(
                name: "FreqTargetLow",
                table: "FT14",
                newName: "Formula_F");
        }
    }
}
