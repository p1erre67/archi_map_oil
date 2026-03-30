namespace PriceWatch.Modules.Cards.Domain.ValueObjects;

public sealed record CardId(Guid Value)
{
    public static CardId New() => new(Guid.NewGuid());
    public static CardId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
