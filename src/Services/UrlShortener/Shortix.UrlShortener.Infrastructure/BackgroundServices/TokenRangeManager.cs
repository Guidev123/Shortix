using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Errors;
using Shortix.UrlShortener.Core.Exceptions;
using Shortix.UrlShortener.Core.Interfaces;
using System.Net.Http.Json;

namespace Shortix.UrlShortener.Infrastructure.BackgroundServices
{
    internal sealed class TokenRangeManager(
        ILogger<TokenRangeManager> logger,
        ITokenService tokenService,
        ITokenRangeApiService tokenRangeApiService
        ) : IHostedService
    {
        private readonly string _machineIdentifier = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                                                  ?? Guid.NewGuid().ToString("O");

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("TokenRangeManager started.");
            }

            tokenService.ReachingRangeLimit += async (sender, args) =>
            {
                await AssignNewRangeAsync(cancellationToken);
            };

            await AssignNewRangeAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("TokenRangeManager stopped.");
            return Task.CompletedTask;
        }

        private async Task AssignNewRangeAsync(CancellationToken cancellationToken)
        {
            var response = await tokenRangeApiService.AssignRangeAsync(_machineIdentifier, cancellationToken);

            if (response.IsFailure)
            {
                throw new FailToGetTokenRangeException(nameof(StartAsync), response.Error);
            }

            var range = response.Value;

            tokenService.AssignRange(new TokenRangeRequest(range.Start, range.End));

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Assigned token range: {Start} - {End} to machine {MachineIdentifier}", range.Start, range.End, _machineIdentifier);
            }
        }
    }
}