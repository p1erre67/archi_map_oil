using MediatR;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Cards.Application.Commands.RecordFuelTransaction;

public sealed record RecordFuelTransactionCommand(
    Guid CardId,
    string FuelType,
    decimal Liters,
    decimal PricePerLiter,
    string StationId) : IRequest<Result>;
