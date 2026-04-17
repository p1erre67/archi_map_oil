using FluentValidation;

namespace PriceWatch.Modules.Prices.Application.Queries.GetStationPrices;

internal sealed class GetStationPricesQueryValidator : AbstractValidator<GetStationPricesQuery>
{
    public GetStationPricesQueryValidator()
    {
        RuleFor(x => x.ExternalStationId)
            .NotEmpty()
            .WithMessage("L'identifiant de station est requis.")
            .MaximumLength(100)
            .WithMessage("L'identifiant de station ne peut pas depasser 100 caracteres.");
    }
}
