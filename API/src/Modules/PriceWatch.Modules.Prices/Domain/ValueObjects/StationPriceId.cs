namespace PriceWatch.Modules.Prices.Domain.ValueObjects;

public sealed record StationPriceId(Guid Value)
{
    public static StationPriceId New() => new(Guid.NewGuid());
    public static StationPriceId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
