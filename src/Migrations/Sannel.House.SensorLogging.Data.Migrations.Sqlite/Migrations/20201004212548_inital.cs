using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sannel.House.SensorLogging.Data.Migrations.Sqlite.Migrations
{
    public partial class inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "SensorEntries",
                columns: table => new
                {
                    SensorEntryId = table.Column<Guid>(nullable: false),
                    LocalDeviceId = table.Column<Guid>(nullable: false),
                    SensorType = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorEntries", x => x.SensorEntryId);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    SensorReadingId = table.Column<Guid>(nullable: false),
                    SensorEntryId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.SensorReadingId);
                    table.ForeignKey(
                        name: "FK_SensorReadings_SensorEntries_SensorEntryId",
                        column: x => x.SensorEntryId,
                        principalTable: "SensorEntries",
                        principalColumn: "SensorEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_LocalDeviceId",
                table: "SensorEntries",
                column: "LocalDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_SensorType",
                table: "SensorEntries",
                column: "SensorType");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_Name",
                table: "SensorReadings",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_SensorEntryId",
                table: "SensorReadings",
                column: "SensorEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "SensorEntries");
        }
    }
}
