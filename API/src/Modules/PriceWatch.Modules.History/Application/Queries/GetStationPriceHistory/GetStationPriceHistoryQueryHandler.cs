using MediatR;
using PriceWatch.Modules.History.Application.DTOs;
using PriceWatch.Modules.History.Domain.Errors;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.History.Application.Queries.GetStationPriceHistory;

internal sealed class GetStationPriceHistoryQueryHandler
    : IRequestHandler<GetStationPriceHistoryQuery, Result<IReadOnlyList<PriceRecordDto>>>
{
    private readonly IPriceRecordRepository _repository;

    public GetStationPriceHistoryQueryHandler(IPriceRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<PriceRecordDto>>> Handle(
        GetStationPriceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _repository.GetByStationAsync(
            request.ExternalStationId,
            request.FuelType,
            cancellationToken);

        if (records.Count == 0)
            return HistoryErrors.Station.NotFound(request.ExternalStationId);

        var dtos = records.Select(r => new PriceRecordDto(
            r.Id.Value,
            r.ExternalStationId,
            r.StationName,
            r.City,
            r.FuelType,
            r.PricePerLiter,
            r.RecordedAt)).ToList();

        return dtos;
    }
}
