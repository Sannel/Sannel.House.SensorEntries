using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sannel.House.SensorLogging.Data.Migrations.Sqlite.Migrations
{
    public partial class Inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorEntries",
                columns: table => new
                {
                    SensorEntryId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<int>(nullable: false),
                    SensorType = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(nullable: false),
                    Values = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorEntries", x => x.SensorEntryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_DeviceId",
                table: "SensorEntries",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorEntries_SensorType",
                table: "SensorEntries",
                column: "SensorType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorEntries");
        }
    }
}
