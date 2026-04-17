using FluentValidation;

namespace PriceWatch.Modules.Prices.Application.Queries.GetCheapestStations;

internal sealed class GetCheapestStationsQueryValidator : AbstractValidator<GetCheapestStationsQuery>
{
    public GetCheapestStationsQueryValidator()
    {
        RuleFor(x => x.FuelType)
            .NotEmpty()
            .WithMessage("Le type de carburant est requis.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("La limite doit etre comprise entre 1 et 100.");
    }
}
