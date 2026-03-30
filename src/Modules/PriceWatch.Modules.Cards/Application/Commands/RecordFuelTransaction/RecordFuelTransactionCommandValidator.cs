using FluentValidation;

namespace PriceWatch.Modules.Cards.Application.Commands.RecordFuelTransaction;

internal sealed class RecordFuelTransactionCommandValidator : AbstractValidator<RecordFuelTransactionCommand>
{
    private static readonly string[] ValidFuelTypes = ["SP95", "SP98", "Gazole", "E10", "E85", "GPLc"];

    public RecordFuelTransactionCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("Card ID is required.");

        RuleFor(x => x.FuelType)
            .NotEmpty().WithMessage("Fuel type is required.")
            .Must(ft => ValidFuelTypes.Contains(ft))
            .WithMessage($"Fuel type must be one of: {string.Join(", ", ValidFuelTypes)}.");

        RuleFor(x => x.Liters)
            .GreaterThan(0).WithMessage("Liters must be greater than zero.");

        RuleFor(x => x.PricePerLiter)
            .GreaterThan(0).WithMessage("Price per liter must be greater than zero.");

        RuleFor(x => x.StationId)
            .NotEmpty().WithMessage("Station ID is required.");
    }
}
