using FluentValidation;
using MediatR;
using ValidationException = Auth.Application.Common.Exceptions.ValidationException;
using Microsoft.Extensions.Logging;
using Auth.Application.Common.Interfaces;

namespace Auth.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILoggerService _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILoggerService logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                _logger.LogWarning("Validation failed for {RequestType}. Errors: {Errors}", 
                    typeof(TRequest).Name,
                    string.Join(", ", failures.Select(f => f.ErrorMessage)));

                throw new ValidationException(failures);
            }
        }

        return await next();
    }
} 