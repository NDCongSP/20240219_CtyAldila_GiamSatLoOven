using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class updateBangHienThiMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "RealtimeDisplay",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RealtimeDisplay",
                table: "RealtimeDisplay",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RealtimeDisplay",
                table: "RealtimeDisplay");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RealtimeDisplay");
        }
    }
}
