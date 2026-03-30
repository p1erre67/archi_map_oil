using FluentValidation;

namespace PriceWatch.Modules.Cards.Application.Commands.CreateCard;

internal sealed class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.HolderName)
            .NotEmpty().WithMessage("Holder name is required.")
            .MaximumLength(200).WithMessage("Holder name must not exceed 200 characters.");
    }
}
