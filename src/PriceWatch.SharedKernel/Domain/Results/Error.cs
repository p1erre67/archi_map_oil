namespace PriceWatch.SharedKernel.Domain.Results;

/// <summary>
/// Represents a domain or application error with a machine-readable code.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string code, string description) =>
        new($"NotFound.{code}", description);

    public static Error Validation(string code, string description) =>
        new($"Validation.{code}", description);

    public static Error Conflict(string code, string description) =>
        new($"Conflict.{code}", description);

    public static Error Unauthorized(string code, string description) =>
        new($"Unauthorized.{code}", description);

    public override string ToString() => $"[{Code}] {Description}";
}
