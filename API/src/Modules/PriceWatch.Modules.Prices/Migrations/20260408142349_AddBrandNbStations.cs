using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceWatch.Modules.Prices.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandNbStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "nb_stations",
                table: "prices_brands",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nb_stations",
                table: "prices_brands");
        }
    }
}
