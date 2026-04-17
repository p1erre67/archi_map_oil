using FluentValidation;

namespace PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;

internal sealed class SyncStationPricesCommandValidator : AbstractValidator<SyncStationPricesCommand>
{
    public SyncStationPricesCommandValidator()
    {
        RuleFor(x => x.Stations)
            .NotNull().WithMessage("La liste de stations ne peut pas etre null.");

        RuleFor(x => x.Stations.Count)
            .LessThanOrEqualTo(500)
            .When(x => x.Stations is not null)
            .WithMessage("La liste de stations ne peut pas depasser 500 elements.");
    }
}
