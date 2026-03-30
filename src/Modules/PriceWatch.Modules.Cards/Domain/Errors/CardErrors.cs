using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Domain.Errors;

public static class CardErrors
{
    public static Error NotFound(Guid cardId) =>
        Error.NotFound("Card", $"Card with ID '{cardId}' was not found.");

    public static readonly Error InvalidTopUpAmount =
        Error.Validation("Card.InvalidTopUpAmount", "Top-up amount must be greater than zero.");

    public static readonly Error CardBlocked =
        Error.Conflict("Card.Blocked", "This card is blocked and cannot be used.");

    public static readonly Error AlreadyBlocked =
        Error.Conflict("Card.AlreadyBlocked", "This card is already blocked.");

    public static readonly Error InsufficientBalance =
        Error.Conflict("Card.InsufficientBalance", "Insufficient balance to complete the fuel transaction.");

    public static readonly Error InvalidFuelAmount =
        Error.Validation("Card.InvalidFuelAmount", "Fuel amount must be greater than zero.");

    public static readonly Error InvalidPricePerLiter =
        Error.Validation("Card.InvalidPricePerLiter", "Price per liter must be greater than zero.");
}
