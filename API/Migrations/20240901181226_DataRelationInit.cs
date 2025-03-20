using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSI_Nuro.Migrations
{
    /// <inheritdoc />
    public partial class DataRelationInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArchiveRecords",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bt = table.Column<double>(type: "float", nullable: false),
                    BxGSM = table.Column<double>(type: "float", nullable: false),
                    ByGSM = table.Column<double>(type: "float", nullable: false),
                    BzGSM = table.Column<double>(type: "float", nullable: false),
                    Density = table.Column<double>(type: "float", nullable: false),
                    Speed = table.Column<double>(type: "float", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    Intensity = table.Column<double>(type: "float", nullable: false),
                    Declination = table.Column<double>(type: "float", nullable: false),
                    Inclination = table.Column<double>(type: "float", nullable: false),
                    North = table.Column<double>(type: "float", nullable: false),
                    East = table.Column<double>(type: "float", nullable: false),
                    Vertical = table.Column<double>(type: "float", nullable: false),
                    Horizontal = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveRecords", x => x.Timestamp);
                });

            migrationBuilder.CreateTable(
                name: "ReconnectionRecords",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BxGSM = table.Column<float>(type: "real", nullable: false),
                    ByGSM = table.Column<float>(type: "real", nullable: false),
                    BzGSM = table.Column<float>(type: "real", nullable: false),
                    Bt = table.Column<float>(type: "real", nullable: false),
                    Density = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconnectionRecords", x => x.Timestamp);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveRecords");

            migrationBuilder.DropTable(
                name: "ReconnectionRecords");
        }
    }
}
