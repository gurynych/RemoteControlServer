using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RemoteControlServer.Migrations
{
    /// <inheritdoc />
    public partial class Addemailremovesaltfildsuserclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "Users",
                newName: "Email");

            migrationBuilder.AddColumn<byte[]>(
                name: "PrivateKey",
                table: "Users",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Salt");
        }
    }
}
