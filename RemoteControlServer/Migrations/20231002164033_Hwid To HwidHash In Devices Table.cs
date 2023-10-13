using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteControlServer.Migrations
{
    /// <inheritdoc />
    public partial class HwidToHwidHashInDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Hwid",
                table: "Devices",
                newName: "HwidHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HwidHash",
                table: "Devices",
                newName: "Hwid");
        }
    }
}
