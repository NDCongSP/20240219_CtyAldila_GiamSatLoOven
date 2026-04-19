using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class addEntitiesPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevoReportShaftVm");

            migrationBuilder.DropTable(
                name: "RevoReportStepVm");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Actions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleToPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermisionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermisionDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMachine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleToPermissions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RoleToPermissions");

            migrationBuilder.CreateTable(
                name: "RevoReportShaftVm",
                columns: table => new
                {
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HighlightIncomplete = table.Column<bool>(type: "bit", nullable: false),
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourBucket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAutoRollingShaft = table.Column<int>(type: "int", nullable: false),
                    IsShaftFinished = table.Column<bool>(type: "bit", nullable: false),
                    Mandrel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevoId = table.Column<int>(type: "int", nullable: true),
                    RevoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftNo = table.Column<long>(type: "bigint", nullable: false),
                    ShaftNum = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StepCount = table.Column<long>(type: "bigint", nullable: false),
                    Stt = table.Column<long>(type: "bigint", nullable: false),
                    TotalTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    TotalTimeText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RevoReportStepVm",
                columns: table => new
                {
                    DisplayEndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisplayStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighlightIncomplete = table.Column<bool>(type: "bit", nullable: false),
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourBucketDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAutoRolling = table.Column<int>(type: "int", nullable: false),
                    IsShaftFinished = table.Column<bool>(type: "bit", nullable: true),
                    Mandrel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Part = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevoId = table.Column<int>(type: "int", nullable: true),
                    RevoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShaftNo = table.Column<long>(type: "bigint", nullable: false),
                    ShaftNum = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StepDisplay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stt = table.Column<long>(type: "bigint", nullable: false),
                    Work = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
