namespace PriceWatch.SharedKernel.Domain.Primitives;

/// <summary>
/// Reusable guard clauses for domain invariants.
/// Ensures entities cannot be created or mutated into an invalid state.
/// </summary>
public static class Guard
{
    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
    }

    public static void AgainstNegativeOrZero(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
    }

    public static void AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must not be negative.");
    }

    public static void AgainstOutOfRange(double value, double min, double max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
    }
}
