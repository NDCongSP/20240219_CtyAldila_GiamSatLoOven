using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class updateEfFT14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FreqTargetLow",
                table: "FT14",
                newName: "Freq_UL");

            migrationBuilder.RenameColumn(
                name: "FreqTargetHight",
                table: "FT14",
                newName: "Freq_LL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Freq_UL",
                table: "FT14",
                newName: "FreqTargetLow");

            migrationBuilder.RenameColumn(
                name: "Freq_LL",
                table: "FT14",
                newName: "FreqTargetHight");
        }
    }
}
