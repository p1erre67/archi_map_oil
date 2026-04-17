using FluentValidation;

namespace PriceWatch.Modules.History.Application.Queries.GetStationPriceHistory;

internal sealed class GetStationPriceHistoryQueryValidator : AbstractValidator<GetStationPriceHistoryQuery>
{
    public GetStationPriceHistoryQueryValidator()
    {
        RuleFor(x => x.ExternalStationId)
            .NotEmpty()
            .WithMessage("L'identifiant de station est requis.")
            .MaximumLength(100)
            .WithMessage("L'identifiant de station ne peut pas depasser 100 caracteres.");

        RuleFor(x => x.FuelType)
            .MaximumLength(20)
            .When(x => x.FuelType is not null)
            .WithMessage("Le type de carburant ne peut pas depasser 20 caracteres.");
    }
}
