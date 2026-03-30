namespace PriceWatch.Modules.Prices.Domain.Entities;

/// <summary>
/// Owned entity representing the price of a specific fuel type at a station.
/// Owned by StationPrice; has no independent lifecycle.
/// </summary>
public sealed class FuelPrice
{
    public string FuelType { get; private set; }
    public decimal PricePerLiter { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private FuelPrice(string fuelType, decimal pricePerLiter, DateTime updatedAt)
    {
        FuelType = fuelType;
        PricePerLiter = pricePerLiter;
        UpdatedAt = updatedAt;
    }

    public static FuelPrice Create(string fuelType, decimal pricePerLiter, DateTime updatedAt)
        => new(fuelType, pricePerLiter, updatedAt);

    public void UpdatePrice(decimal pricePerLiter, DateTime updatedAt)
    {
        PricePerLiter = pricePerLiter;
        UpdatedAt = updatedAt;
    }

#pragma warning disable CS8618
    private FuelPrice() { }
#pragma warning restore CS8618
}
