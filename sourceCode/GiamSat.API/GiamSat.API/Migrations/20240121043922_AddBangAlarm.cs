using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBangAlarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACK = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ACKDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlarmSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CCString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: true),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<int>(type: "int", nullable: true),
                    EnableSSL = table.Column<bool>(type: "bit", nullable: true),
                    CredentialEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CredentialPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timeout = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmLog");

            migrationBuilder.DropTable(
                name: "AlarmSettings");
        }
    }
}
