using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteControlServer.Migrations
{
    /// <inheritdoc />
    public partial class Addeddeviceinfoproperies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceManufacturer",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevicePlatform",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevicePlatformVersion",
                table: "Devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "Devices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceManufacturer",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DevicePlatform",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DevicePlatformVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Devices");
        }
    }
}
