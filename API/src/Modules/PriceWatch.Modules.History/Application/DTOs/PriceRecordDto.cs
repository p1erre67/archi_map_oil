namespace PriceWatch.Modules.History.Application.DTOs;

public sealed record PriceRecordDto(
    Guid Id,
    string ExternalStationId,
    string StationName,
    string City,
    string FuelType,
    decimal PricePerLiter,
    DateTime RecordedAt);
