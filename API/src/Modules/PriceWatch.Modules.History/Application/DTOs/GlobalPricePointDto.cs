namespace PriceWatch.Modules.History.Application.DTOs;

/// <summary>
/// Represents an aggregated price point for a given date (average across all stations).
/// </summary>
public sealed record GlobalPricePointDto(
    DateTime Date,
    decimal AveragePrice,
    decimal MinPrice,
    decimal MaxPrice,
    int StationCount);
