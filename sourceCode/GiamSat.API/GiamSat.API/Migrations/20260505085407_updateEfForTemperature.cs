using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiamSat.API.Migrations
{
    /// <inheritdoc />
    public partial class updateEfForTemperature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleToPermissions",
                table: "RoleToPermissions");

            migrationBuilder.RenameColumn(
                name: "PV",
                table: "FT13",
                newName: "PV_Normal");

            migrationBuilder.RenameColumn(
                name: "C001",
                table: "FT11",
                newName: "C000");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RoleToPermissions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PV_Alarm",
                table: "FT13",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "FT12",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleToPermissions",
                table: "RoleToPermissions",
                columns: new[] { "RoleId", "PermissionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleToPermissions",
                table: "RoleToPermissions");

            migrationBuilder.DropColumn(
                name: "PV_Alarm",
                table: "FT13");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "FT12");

            migrationBuilder.RenameColumn(
                name: "PV_Normal",
                table: "FT13",
                newName: "PV");

            migrationBuilder.RenameColumn(
                name: "C000",
                table: "FT11",
                newName: "C001");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RoleToPermissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleToPermissions",
                table: "RoleToPermissions",
                column: "Id");
        }
    }
}
