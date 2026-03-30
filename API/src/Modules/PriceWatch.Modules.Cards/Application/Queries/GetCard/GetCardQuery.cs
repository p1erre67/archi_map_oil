using MediatR;
using PriceWatch.Modules.Cards.Application.DTOs;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Queries.GetCard;

public sealed record GetCardQuery(Guid CardId) : IRequest<Result<CardDto>>;
