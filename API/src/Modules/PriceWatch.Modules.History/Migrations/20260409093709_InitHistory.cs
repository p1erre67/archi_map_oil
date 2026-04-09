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
            migrationBuilder.CreateTable(
                name: "history_price_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_station_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    station_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price_per_liter = table.Column<decimal>(type: "numeric(8,3)", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_price_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_external_station_id",
                table: "history_price_records",
                column: "external_station_id");

            migrationBuilder.CreateIndex(
                name: "IX_history_price_records_external_station_id_fuel_type_recorde~",
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
