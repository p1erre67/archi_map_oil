using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceWatch.Modules.Prices.Migrations
{
    /// <inheritdoc />
    public partial class AddBrands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "brand_id",
                table: "prices_station_prices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prices_brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    short_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices_brands", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_prices_station_prices_brand_id",
                table: "prices_station_prices",
                column: "brand_id");

            migrationBuilder.AddForeignKey(
                name: "FK_prices_station_prices_prices_brands_brand_id",
                table: "prices_station_prices",
                column: "brand_id",
                principalTable: "prices_brands",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prices_station_prices_prices_brands_brand_id",
                table: "prices_station_prices");

            migrationBuilder.DropTable(
                name: "prices_brands");

            migrationBuilder.DropIndex(
                name: "IX_prices_station_prices_brand_id",
                table: "prices_station_prices");

            migrationBuilder.DropColumn(
                name: "brand_id",
                table: "prices_station_prices");
        }
    }
}
