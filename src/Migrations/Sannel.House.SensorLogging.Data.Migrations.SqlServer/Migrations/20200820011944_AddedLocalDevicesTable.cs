using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sannel.House.SensorLogging.Data.Migrations.SqlServer.Migrations
{
    public partial class AddedLocalDevicesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SensorEntries_DeviceId",
                table: "SensorEntries");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "SensorEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "LocalDeviceId",
                table: "SensorEntries",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    LocalDeviceId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<int>(nullable: true),
                    Uuid = table.Column<Guid>(nullable: true),
                    MacAddress = table.Column<long>(nullable: true),
                    Manufacture = table.Column<string>(nullable: true),
                    ManufactureId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.LocalDeviceId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_LocalDeviceId",
                table: "SensorEntries",
                column: "LocalDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_MacAddress",
                table: "Devices",
                column: "MacAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Uuid",
                table: "Devices",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Manufacture_ManufactureId",
                table: "Devices",
                columns: new[] { "Manufacture", "ManufactureId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_SensorEntries_LocalDeviceId",
                table: "SensorEntries");

            migrationBuilder.DropColumn(
                name: "LocalDeviceId",
                table: "SensorEntries");

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "SensorEntries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_DeviceId",
                table: "SensorEntries",
                column: "DeviceId");
        }
    }
}
