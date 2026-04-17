using FluentValidation;

namespace PriceWatch.Modules.History.Application.Queries.GetGlobalPriceHistory;

internal sealed class GetGlobalPriceHistoryQueryValidator : AbstractValidator<GetGlobalPriceHistoryQuery>
{
    public GetGlobalPriceHistoryQueryValidator()
    {
        RuleFor(x => x.FuelType)
            .NotEmpty()
            .WithMessage("Le type de carburant est requis.");

        RuleFor(x => x.From)
            .LessThan(x => x.To)
            .WithMessage("La date de debut doit etre anterieure a la date de fin.");

    }
}
