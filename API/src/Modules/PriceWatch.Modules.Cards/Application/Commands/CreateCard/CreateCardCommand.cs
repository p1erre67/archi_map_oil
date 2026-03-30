using MediatR;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.CreateCard;

public sealed record CreateCardCommand(string HolderName) : IRequest<Result<Guid>>;
