using FluentValidation;

namespace PriceWatch.Modules.Cards.Application.Commands.TopUpCard;

internal sealed class TopUpCardCommandValidator : AbstractValidator<TopUpCardCommand>
{
    public TopUpCardCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("Card ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Top-up amount must be greater than zero.");
    }
}
