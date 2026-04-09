using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PriceWatch.Modules.Prices.Migrations
{
    /// <inheritdoc />
    public partial class InitPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prices_brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    short_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nb_stations = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prices_station_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_station_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    station_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    brand_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices_station_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_prices_station_prices_prices_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "prices_brands",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "prices_fuel_prices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fuel_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price_per_liter = table.Column<decimal>(type: "numeric(8,3)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    station_price_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices_fuel_prices", x => x.id);
                    table.ForeignKey(
                        name: "FK_prices_fuel_prices_prices_station_prices_station_price_id",
                        column: x => x.station_price_id,
                        principalTable: "prices_station_prices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prices_fuel_prices_station_price_id",
                table: "prices_fuel_prices",
                column: "station_price_id");

            migrationBuilder.CreateIndex(
                name: "IX_prices_station_prices_brand_id",
                table: "prices_station_prices",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_prices_station_prices_external_station_id",
                table: "prices_station_prices",
                column: "external_station_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prices_fuel_prices");

            migrationBuilder.DropTable(
                name: "prices_station_prices");

            migrationBuilder.DropTable(
                name: "prices_brands");
        }
    }
}
