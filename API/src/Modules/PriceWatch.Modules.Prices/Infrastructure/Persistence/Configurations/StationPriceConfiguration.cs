using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.ValueObjects;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence.Configurations;

internal sealed class StationPriceConfiguration : IEntityTypeConfiguration<StationPrice>
{
    public void Configure(EntityTypeBuilder<StationPrice> builder)
    {
        builder.ToTable("prices_station_prices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => StationPriceId.From(value))
            .HasColumnName("id");

        builder.Property(x => x.ExternalStationId)
            .HasColumnName("external_station_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.ExternalStationId)
            .IsUnique();

        builder.Property(x => x.StationName)
            .HasColumnName("station_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .IsRequired();

        builder.Property(x => x.LastUpdated)
            .HasColumnName("last_updated")
            .IsRequired();

        builder.Property(x => x.BrandId)
            .HasColumnName("brand_id");

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .IsRequired(false);

        builder.OwnsMany(x => x.FuelPrices, fp =>
        {
            fp.ToTable("prices_fuel_prices");

            fp.WithOwner().HasForeignKey("station_price_id");

            fp.Property<int>("id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            fp.HasKey("id");

            fp.Property(x => x.FuelType)
                .HasColumnName("fuel_type")
                .HasMaxLength(20)
                .IsRequired();

            fp.Property(x => x.PricePerLiter)
                .HasColumnName("price_per_liter")
                .HasColumnType("decimal(8,3)")
                .IsRequired();

            fp.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
