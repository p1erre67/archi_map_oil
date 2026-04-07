using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.History.Domain.Errors;

public static class HistoryErrors
{
    public static class Station
    {
        public static Error NotFound(string externalStationId) =>
            Error.NotFound("Station", $"No history found for station '{externalStationId}'.");
    }
}
