using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class initBangHienThiMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "RealtimeDisplay",
                columns: table => new
                {
                    DisplayData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

        }
    }
}
