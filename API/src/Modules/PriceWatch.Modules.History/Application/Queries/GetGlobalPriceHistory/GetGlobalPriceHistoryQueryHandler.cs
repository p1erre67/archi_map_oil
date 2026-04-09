using MediatR;
using PriceWatch.Modules.History.Application.DTOs;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.History.Application.Queries.GetGlobalPriceHistory;

internal sealed class GetGlobalPriceHistoryQueryHandler
    : IRequestHandler<GetGlobalPriceHistoryQuery, Result<IReadOnlyList<GlobalPricePointDto>>>
{
    private readonly IPriceRecordRepository _repository;

    public GetGlobalPriceHistoryQueryHandler(IPriceRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<GlobalPricePointDto>>> Handle(
        GetGlobalPriceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Postgres exige des DateTime UTC pour les colonnes timestamptz
        var fromUtc = DateTime.SpecifyKind(request.From, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To, DateTimeKind.Utc);

        var records = await _repository.GetGlobalAveragesAsync(
            request.FuelType,
            fromUtc,
            toUtc,
            request.StationIds,
            cancellationToken);

        // Agrège par date (jour) pour donner une courbe d'évolution globale
        var points = records
            .GroupBy(r => r.RecordedAt.Date)
            .Select(g => new GlobalPricePointDto(
                g.Key,
                Math.Round(g.Average(r => r.PricePerLiter), 3),
                g.Min(r => r.PricePerLiter),
                g.Max(r => r.PricePerLiter),
                g.Select(r => r.ExternalStationId).Distinct().Count()))
            .OrderBy(p => p.Date)
            .ToList();

        return points;
    }
}
