using FluentValidation;
using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior for validation using FluentValidation
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
        where TResponse : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                
                // If TResponse is a Result type, return failure
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var failureMethod = typeof(Result<>).MakeGenericType(resultType).GetMethod("Failure");
                    return (TResponse)failureMethod!.Invoke(null, new object[] { errorMessage })!;
                }

                if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(errorMessage);
                }

                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
