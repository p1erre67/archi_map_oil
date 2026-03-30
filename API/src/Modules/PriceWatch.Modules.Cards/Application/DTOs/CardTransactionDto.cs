namespace PriceWatch.Modules.Cards.Application.DTOs;

public sealed record CardTransactionDto(
    Guid TransactionId,
    string Type,
    decimal AmountCharged,
    decimal? LitersFueled,
    string? FuelType,
    string? StationId,
    DateTime OccurredAt);
