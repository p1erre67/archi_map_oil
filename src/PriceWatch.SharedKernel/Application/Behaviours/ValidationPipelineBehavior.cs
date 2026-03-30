using FluentValidation;
using MediatR;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.SharedKernel.Application.Behaviours;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before every command/query.
/// Constrained to requests whose response extends Result, so we can return a typed failure
/// without reaching the handler.
/// </summary>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var error = Error.Validation(
            failures[0].PropertyName,
            string.Join("; ", failures.Select(f => f.ErrorMessage)));

        return Result.CreateFailure<TResponse>(error);
    }
}
