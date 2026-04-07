namespace PriceWatch.Modules.History.Domain.ValueObjects;

public sealed record PriceRecordId(Guid Value)
{
    public static PriceRecordId New() => new(Guid.NewGuid());
    public static PriceRecordId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
