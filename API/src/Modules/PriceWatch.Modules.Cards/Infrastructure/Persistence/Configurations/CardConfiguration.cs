using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PriceWatch.Modules.Cards.Domain.Entities;
using PriceWatch.Modules.Cards.Domain.Enums;
using PriceWatch.Modules.Cards.Domain.ValueObjects;

namespace PriceWatch.Modules.Cards.Infrastructure.Persistence.Configurations;

internal sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("cards_cards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => CardId.From(value))
            .HasColumnName("id");

        builder.Property(x => x.CardNumber)
            .HasConversion(cn => cn.Value, value => CardNumber.From(value))
            .HasColumnName("card_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.CardNumber)
            .IsUnique();

        builder.Property(x => x.HolderName)
            .HasColumnName("holder_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasColumnName("balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(s => (int)s, v => (CardStatus)v)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.OwnsMany(c => c.Transactions, t =>
        {
            t.ToTable("cards_transactions");

            t.WithOwner().HasForeignKey("CardId");

            t.Property(x => x.TransactionId)
                .HasColumnName("transaction_id")
                .IsRequired();

            t.HasKey(x => x.TransactionId);

            t.Property(x => x.Type)
                .HasConversion(tp => (int)tp, v => (TransactionType)v)
                .HasColumnName("type")
                .IsRequired();

            t.Property(x => x.AmountCharged)
                .HasColumnName("amount_charged")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            t.Property(x => x.LitersFueled)
                .HasColumnName("liters_fueled")
                .HasColumnType("decimal(10,3)");

            t.Property(x => x.FuelType)
                .HasColumnName("fuel_type")
                .HasMaxLength(20);

            t.Property(x => x.StationId)
                .HasColumnName("station_id")
                .HasMaxLength(100);

            t.Property(x => x.OccurredAt)
                .HasColumnName("occurred_at")
                .IsRequired();
        });

        builder.Ignore(x => x.DomainEvents);
    }
}
