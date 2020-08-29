using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sannel.House.SensorLogging.Data.Migrations.SqlServer.Migrations
{
    public partial class UpdateValuesStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Values",
                table: "SensorEntries");

            migrationBuilder.CreateTable(
                name: "SensorReading",
                columns: table => new
                {
                    SensorReadingId = table.Column<Guid>(nullable: false),
                    SensorEntryId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReading", x => x.SensorReadingId);
                    table.ForeignKey(
                        name: "FK_SensorReading_SensorEntries_SensorEntryId",
                        column: x => x.SensorEntryId,
                        principalTable: "SensorEntries",
                        principalColumn: "SensorEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReading_Name",
                table: "SensorReading",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReading_SensorEntryId",
                table: "SensorReading",
                column: "SensorEntryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReading");

            migrationBuilder.AddColumn<string>(
                name: "Values",
                table: "SensorEntries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
