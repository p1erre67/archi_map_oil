namespace PriceWatch.Modules.Cards.Application.DTOs;

public sealed record CardDto(
    Guid CardId,
    string CardNumber,
    string HolderName,
    decimal Balance,
    string Status,
    DateTime CreatedAt);
