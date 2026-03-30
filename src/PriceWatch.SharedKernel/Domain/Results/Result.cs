namespace PriceWatch.SharedKernel.Domain.Results;

/// <summary>
/// Represents the outcome of an operation that can succeed or fail.
/// Avoids using exceptions for expected business failures (e.g. NotFound, Validation).
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    /// <summary>
    /// Creates a failure result of type TResult (Result or Result&lt;T&gt;) from an Error.
    /// Used internally by ValidationPipelineBehavior to return the right Result type
    /// without knowing TResult at compile time.
    /// </summary>
    internal static TResult CreateFailure<TResult>(Error error) where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
            return (TResult)(object)Failure(error);

        var valueType = typeof(TResult).GetGenericArguments()[0];
        var method = typeof(Result<>)
            .MakeGenericType(valueType)
            .GetMethod(nameof(Result<object>.Failure), [typeof(Error)])!;

        return (TResult)method.Invoke(null, [error])!;
    }
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value) : base(true, Error.None) => _value = value;
    private Result(Error error) : base(false, error) { }

    /// <summary>
    /// Gets the value. Throws if the result is a failure.
    /// Always check IsSuccess before accessing Value.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public new static Result<TValue> Failure(Error error) => new(error);

    /// <summary>
    /// Allows implicit conversion from a value to a successful result.
    /// Enables writing: return myValue; instead of: return Result.Success(myValue);
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Allows implicit conversion from an Error to a failed result.
    /// Enables writing: return MyErrors.NotFound; instead of: return Result.Failure(MyErrors.NotFound);
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
