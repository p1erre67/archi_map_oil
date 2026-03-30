using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.Modules.Prices.Domain.Errors;

public static class PricesErrors
{
    public static class Station
    {
        public static Error NotFound(string externalStationId) =>
            Error.NotFound("Station", $"Station with external ID '{externalStationId}' was not found.");

        public static readonly Error InvalidData =
            Error.Validation("Station.InvalidData", "Station data is invalid.");
    }
}
