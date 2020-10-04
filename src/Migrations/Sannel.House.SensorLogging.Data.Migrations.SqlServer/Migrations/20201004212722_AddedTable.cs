using Microsoft.EntityFrameworkCore.Migrations;

namespace Sannel.House.SensorLogging.Data.Migrations.SqlServer.Migrations
{
    public partial class AddedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorReading_SensorEntries_SensorEntryId",
                table: "SensorReading");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorReading",
                table: "SensorReading");

            migrationBuilder.RenameTable(
                name: "SensorReading",
                newName: "SensorReadings");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReading_SensorEntryId",
                table: "SensorReadings",
                newName: "IX_SensorReadings_SensorEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReading_Name",
                table: "SensorReadings",
                newName: "IX_SensorReadings_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorReadings",
                table: "SensorReadings",
                column: "SensorReadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReadings_SensorEntries_SensorEntryId",
                table: "SensorReadings",
                column: "SensorEntryId",
                principalTable: "SensorEntries",
                principalColumn: "SensorEntryId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorReadings_SensorEntries_SensorEntryId",
                table: "SensorReadings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorReadings",
                table: "SensorReadings");

            migrationBuilder.RenameTable(
                name: "SensorReadings",
                newName: "SensorReading");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReadings_SensorEntryId",
                table: "SensorReading",
                newName: "IX_SensorReading_SensorEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_SensorReadings_Name",
                table: "SensorReading",
                newName: "IX_SensorReading_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorReading",
                table: "SensorReading",
                column: "SensorReadingId");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReading_SensorEntries_SensorEntryId",
                table: "SensorReading",
                column: "SensorEntryId",
                principalTable: "SensorEntries",
                principalColumn: "SensorEntryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
