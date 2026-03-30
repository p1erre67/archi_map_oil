using MediatR;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.TopUpCard;

public sealed record TopUpCardCommand(Guid CardId, decimal Amount) : IRequest<Result>;
