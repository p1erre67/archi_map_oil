using MediatR;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.BlockCard;

public sealed record BlockCardCommand(Guid CardId) : IRequest<Result>;
