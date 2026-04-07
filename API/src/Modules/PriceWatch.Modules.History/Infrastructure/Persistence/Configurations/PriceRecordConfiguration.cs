using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Domain.ValueObjects;

namespace PriceWatch.Modules.History.Infrastructure.Persistence.Configurations;

internal sealed class PriceRecordConfiguration : IEntityTypeConfiguration<PriceRecord>
{
    public void Configure(EntityTypeBuilder<PriceRecord> builder)
    {
        builder.ToTable("history_price_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => PriceRecordId.From(value))
            .HasColumnName("id");

        builder.Property(x => x.ExternalStationId)
            .HasColumnName("external_station_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.ExternalStationId);

        builder.Property(x => x.StationName)
            .HasColumnName("station_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.FuelType)
            .HasColumnName("fuel_type")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.FuelType);

        builder.Property(x => x.PricePerLiter)
            .HasColumnName("price_per_liter")
            .HasColumnType("decimal(8,3)")
            .IsRequired();

        builder.Property(x => x.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.HasIndex(x => x.RecordedAt);

        // Index composite pour les requêtes fréquentes
        builder.HasIndex(x => new { x.ExternalStationId, x.FuelType, x.RecordedAt });
    }
}
