using Microsoft.Extensions.Logging;
using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Errors;
using Shortix.UrlShortener.Core.Interfaces;
using System.Net.Http.Json;

namespace Shortix.UrlShortener.Infrastructure.Services
{
    internal sealed class TokenRangeApiService(IHttpClientFactory httpClientFactory, ILogger<TokenRangeApiService> logger) : ITokenRangeApiService
    {
        private readonly HttpClient _client = httpClientFactory.CreateClient(InfrastructureModule.TokenRangesHttpClientName);

        public async Task<Result<TokenRangeApiResponse>> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken = default)
        {
            var response = await _client.PostAsJsonAsync("api/v1/token-ranges/assign", new { Key = machineIdentifier }, cancellationToken);

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