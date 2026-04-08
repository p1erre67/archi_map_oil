using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceWatch.Modules.History.Migrations
{
    /// <inheritdoc />
    public partial class InitHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "history_price_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    external_station_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    station_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fuel_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price_per_liter = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_price_records", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_external_station_id",
                table: "history_price_records",
                column: "external_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_external_station_id_fuel_type_recorded~",
                table: "history_price_records",
                columns: new[] { "external_station_id", "fuel_type", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_fuel_type",
                table: "history_price_records",
                column: "fuel_type");

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_recorded_at",
                table: "history_price_records",
                column: "recorded_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history_price_records");
        }
    }
}
