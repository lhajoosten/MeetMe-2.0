using FluentValidation;
using MediatR;
using MeetMe.Application.Common.Models;
using System.Reflection;

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
                    // Get all methods named "Failure" and filter to the generic one
                    var failureMethods = typeof(Result).GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => m.Name == "Failure" && m.IsGenericMethodDefinition)
                        .ToArray();
                    
                    if (failureMethods.Length == 1)
                    {
                        var failureMethod = failureMethods[0].MakeGenericMethod(resultType);
                        var result = failureMethod.Invoke(null, new object[] { errorMessage })!;
                        return (TResponse)result;
                    }
                    else
                    {
                        throw new InvalidOperationException("Could not find unique generic Failure method");
                    }
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
