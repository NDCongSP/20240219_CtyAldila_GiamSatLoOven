using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addTableThongTinMeNuoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FT100",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActived = table.Column<int>(type: "int", nullable: false),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT100", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT101",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FT100Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActived = table.Column<int>(type: "int", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT101", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FT100");

            migrationBuilder.DropTable(
                name: "FT101");

           
           
        }
    }
}
