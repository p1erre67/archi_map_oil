using FluentValidation;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

internal sealed class SyncStationPricesCommandValidator : AbstractValidator<SyncStationPricesCommand>
{
    public SyncStationPricesCommandValidator()
    {
        RuleFor(x => x.Stations)
            .NotNull().WithMessage("Stations list must not be null.");
    }
}
