using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addEntityForTemperature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FT10",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C000 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Actived = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT10", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT11",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    C001 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT11", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT12",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateddAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PV = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT12", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FT13",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateddAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PV = table.Column<double>(type: "float", nullable: true),
                    SV_High = table.Column<double>(type: "float", nullable: true),
                    SV_Low = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FT13", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevoReportHourVm",
                columns: table => new
                {
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TotalTimeText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftCountFinishedInHour = table.Column<long>(type: "bigint", nullable: false),
                    IncompleteShaftCountInHour = table.Column<long>(type: "bigint", nullable: false),
                    HighlightIncomplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RevoReportShaftVm",
                columns: table => new
                {
                    ShaftLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevoId = table.Column<int>(type: "int", nullable: true),
                    RevoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourBucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftNo = table.Column<long>(type: "bigint", nullable: false),
                    Stt = table.Column<long>(type: "bigint", nullable: false),
                    ShaftNum = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandrel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TotalTimeText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepCount = table.Column<long>(type: "bigint", nullable: false),
                    IsAutoRollingShaft = table.Column<int>(type: "int", nullable: false),
                    IsShaftFinished = table.Column<bool>(type: "bit", nullable: false),
                    HighlightIncomplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RevoReportStepVm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevoId = table.Column<int>(type: "int", nullable: true),
                    RevoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourBucketDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftNo = table.Column<long>(type: "bigint", nullable: false),
                    ShaftKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftNum = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandrel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayEndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAutoRolling = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stt = table.Column<long>(type: "bigint", nullable: false),
                    IsShaftFinished = table.Column<bool>(type: "bit", nullable: true),
                    HighlightIncomplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FT10");

            migrationBuilder.DropTable(
                name: "FT11");

            migrationBuilder.DropTable(
                name: "FT12");

            migrationBuilder.DropTable(
                name: "FT13");

            migrationBuilder.DropTable(
                name: "RevoReportHourVm");

            migrationBuilder.DropTable(
                name: "RevoReportShaftVm");

            migrationBuilder.DropTable(
                name: "RevoReportStepVm");
        }
    }
}
