using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeetMe.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior for logging requests and performance monitoring
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Handling {RequestName}", requestName);

            try
            {
                var response = await next();
                
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                if (elapsedMilliseconds > 500) // Log if request takes longer than 500ms
                {
                    _logger.LogWarning("Long running request: {RequestName} took {ElapsedMilliseconds} ms", 
                        requestName, elapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("Completed {RequestName} in {ElapsedMilliseconds} ms", 
                        requestName, elapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds} ms", 
                    requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
