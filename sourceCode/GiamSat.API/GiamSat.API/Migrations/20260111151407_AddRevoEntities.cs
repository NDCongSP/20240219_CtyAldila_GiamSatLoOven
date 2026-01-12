using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRevoEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FT08",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C000 = table.Column<int>(type: "int", nullable: true),
                    C001 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT08", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT09",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevoId = table.Column<int>(type: "int", nullable: true),
                    RevoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandrel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MandrelStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepId = table.Column<int>(type: "int", nullable: true),
                    StepName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT09", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FT08");

            migrationBuilder.DropTable(
                name: "FT09");
        }
    }
}
