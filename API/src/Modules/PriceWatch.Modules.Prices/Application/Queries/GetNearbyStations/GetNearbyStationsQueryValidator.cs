using FluentValidation;

namespace PriceWatch.Modules.Prices.Application.Queries.GetNearbyStations;

internal sealed class GetNearbyStationsQueryValidator : AbstractValidator<GetNearbyStationsQuery>
{
    public GetNearbyStationsQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("La latitude doit etre comprise entre -90 et 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("La longitude doit etre comprise entre -180 et 180.");

        RuleFor(x => x.RadiusKm)
            .InclusiveBetween(1, 9)
            .WithMessage("Le rayon doit etre compris entre 1 et 9 km.");
    }
}
