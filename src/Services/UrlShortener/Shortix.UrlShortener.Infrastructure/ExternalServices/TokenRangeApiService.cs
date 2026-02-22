using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Errors;
using Shortix.UrlShortener.Core.Interfaces;
using System.Net.Http.Json;

namespace Shortix.UrlShortener.Infrastructure.ExternalServices
{
    internal sealed class TokenRangeApiService(IHttpClientFactory httpClientFactory, ILogger<TokenRangeApiService> logger) : ITokenRangeApiService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(InfrastructureModule.TokenRangesHttpClientName);

        private static readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
            HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public async Task<Result<TokenRangeApiResponse>> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken = default)
        {
            var machineIdentifierRequest = new { Key = machineIdentifier };

            var response = await _retryPolicy.ExecuteAsync(() => _client.PostAsJsonAsync("api/v1/token-ranges/assign", machineIdentifierRequest, cancellationToken));

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<TokenRangeApiResponse>(TokenRangeErrors.FailToGetTokenRange(machineIdentifier));
            }

            var range = await response.Content.ReadFromJsonAsync<TokenRangeApiResponse>(cancellationToken: cancellationToken);

            if (range is null)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("Failed to deserialize token range response for machine {MachineIdentifier}", machineIdentifier);
                }

                return Result.Failure<TokenRangeApiResponse>(TokenRangeErrors.FailToGetTokenRange(machineIdentifier));
            }

            return Result.Success(range);
        }
    }
}