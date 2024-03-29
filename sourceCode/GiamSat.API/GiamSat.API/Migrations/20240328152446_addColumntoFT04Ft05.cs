using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addColumntoFT04Ft05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hours",
                table: "FT05",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: "FT05",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "FT05",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "FT05",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seconds",
                table: "FT05",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Setpoint",
                table: "FT05",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "StepId",
                table: "FT05",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StepName",
                table: "FT05",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "FT05",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Hours",
                table: "FT04",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: "FT04",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "FT04",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfileName",
                table: "FT04",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Seconds",
                table: "FT04",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Setpoint",
                table: "FT04",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "StepId",
                table: "FT04",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StepName",
                table: "FT04",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hours",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "Minutes",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "Seconds",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "Setpoint",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "StepId",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "StepName",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "FT05");

            migrationBuilder.DropColumn(
                name: "Hours",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "Minutes",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "ProfileName",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "Seconds",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "Setpoint",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "StepId",
                table: "FT04");

            migrationBuilder.DropColumn(
                name: "StepName",
                table: "FT04");
        }
    }
}
