using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.Modules.Prices.Infrastructure.Persistence.Configurations;

internal sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("prices_brands");

        builder.HasKey(x => x.ExternalId);

        builder.Property(x => x.ExternalId)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ShortName)
            .HasColumnName("short_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.NbStations)
            .HasColumnName("nb_stations")
            .IsRequired();
    }
}
