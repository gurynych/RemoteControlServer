using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteControlServer.Migrations
{
    /// <inheritdoc />
    public partial class RenameProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrivateKey",
                table: "Users",
                newName: "AuthToken");

            migrationBuilder.RenameColumn(
                name: "HwidHash",
                table: "Devices",
                newName: "DeviceGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuthToken",
                table: "Users",
                newName: "PrivateKey");

            migrationBuilder.RenameColumn(
                name: "DeviceGuid",
                table: "Devices",
                newName: "HwidHash");
        }
    }
}
