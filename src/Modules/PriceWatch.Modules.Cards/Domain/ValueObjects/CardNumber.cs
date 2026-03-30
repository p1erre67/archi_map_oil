namespace PriceWatch.Modules.Cards.Domain.ValueObjects;

/// <summary>
/// Value object wrapping a card number in format "PW-XXXXXXXX".
/// </summary>
public sealed record CardNumber
{
    public string Value { get; }

    private CardNumber(string value) => Value = value;

    public static CardNumber Generate() =>
        new("PW-" + Guid.NewGuid().ToString("N")[..8].ToUpper());

    public static CardNumber From(string value) => new(value);

    public override string ToString() => Value;
}
