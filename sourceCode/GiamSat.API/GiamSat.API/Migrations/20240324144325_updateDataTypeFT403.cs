using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class updateDataTypeFT403 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "FT03",
                newName: "Actived");

            migrationBuilder.AddColumn<Guid>(
                name: "ZIndex",
                table: "FT04",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZIndex",
                table: "FT04");

            migrationBuilder.RenameColumn(
                name: "Actived",
                table: "FT03",
                newName: "IsActive");
        }
    }
}
