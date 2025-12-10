using Microsoft.Extensions.Logging;
using MidR.Behaviors;
using MidR.Interfaces;
using Serilog.Context;
using Shortix.Commons.Core.Exceptions;
using Shortix.Commons.Core.Results;
using System.Diagnostics;

namespace Shortix.Commons.Core.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IRequestBehavior<TRequest, TResponse>
           where TRequest : IRequest<TResponse>
           where TResponse : Result
    {
        public async Task<TResponse> ExecuteAsync(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestModule = GetRequestModule(typeof(TRequest).FullName!);

            Activity.Current?.SetTag("request.module", requestModule);
            Activity.Current?.SetTag("request.name", requestName);

            var stopwatch = Stopwatch.StartNew();
            using (LogContext.PushProperty("Module", requestModule))
            {
                try
                {
                    logger.LogInformation("Processing request: {RequestName}", requestName);

                    var result = await next();

                    stopwatch.Stop();
                    var executionTime = stopwatch.ElapsedMilliseconds;

                    if (result.IsSuccess)
                    {
                        logger.LogInformation("Request: {RequestName} processed successfully in {ExecutionTimeInMilliseconds}ms",
                            requestName, executionTime);
                    }
                    else
                    {
                        using (LogContext.PushProperty("Error", result.Error, true))
                        {
                            logger.LogError("Request: {RequestName} failed in {ExecutionTimeInMilliseconds}ms",
                                requestName, executionTime);
                        }
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    var executionTime = stopwatch.ElapsedMilliseconds;

                    logger.LogError(ex, "Request: {RequestName} failed in {ExecutionTimeInMilliseconds}ms with unhandled exception", requestName, executionTime);

                    throw new ShortixException(typeof(TRequest).Name, innerException: ex);
                }
            }
        }

        private static string GetRequestModule(string requestName) => requestName.Split('.')[1];
    }
}