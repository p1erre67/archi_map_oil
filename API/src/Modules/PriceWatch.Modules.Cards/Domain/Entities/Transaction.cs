using PriceWatch.Modules.Cards.Domain.Enums;

namespace PriceWatch.Modules.Cards.Domain.Entities;

/// <summary>
/// Owned entity representing a card transaction.
/// Owned by Card; has no independent lifecycle.
/// </summary>
public sealed class Transaction
{
    public Guid TransactionId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal AmountCharged { get; private set; }
    public decimal? LitersFueled { get; private set; }
    public string? FuelType { get; private set; }
    public string? StationId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private Transaction(
        Guid transactionId,
        TransactionType type,
        decimal amountCharged,
        decimal? litersFueled,
        string? fuelType,
        string? stationId,
        DateTime occurredAt)
    {
        TransactionId = transactionId;
        Type = type;
        AmountCharged = amountCharged;
        LitersFueled = litersFueled;
        FuelType = fuelType;
        StationId = stationId;
        OccurredAt = occurredAt;
    }

    public static Transaction CreateTopUp(decimal amount) =>
        new(Guid.NewGuid(), TransactionType.TopUp, amount, null, null, null, DateTime.UtcNow);

    public static Transaction CreateFuelPayment(
        decimal totalCharged,
        decimal liters,
        string fuelType,
        string stationId) =>
        new(Guid.NewGuid(), TransactionType.FuelPayment, totalCharged, liters, fuelType, stationId, DateTime.UtcNow);

#pragma warning disable CS8618
    private Transaction() { }
#pragma warning restore CS8618
}
