using Shortix.Commons.Core.Results;
using Shortix.Redirect.WebApi.Interfaces;
using Shortix.Redirect.WebApi.Models;
using StackExchange.Redis;

namespace Shortix.Redirect.WebApi.Services
{
    public sealed class RedisUrlShortenerService(
        ILogger<RedisUrlShortenerService> logger,
        IUrlShortenerService urlShortenerService,
        IConnectionMultiplexer connectionMultiplexer
        ) : IUrlShortenerService
    {
        private readonly IDatabase _redisDb = connectionMultiplexer.GetDatabase();

        public async Task<Result<ReadLongUrlResponse>> GetLongUrlAsync(string shortUrl, CancellationToken cancellationToken = default)
        {
            var cachedUrl = await _redisDb.StringGetAsync(shortUrl).WaitAsync(cancellationToken);
            if (cachedUrl.HasValue)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("[CACHE HIT] found cached long url for short URL: {ShortUrl}", shortUrl);
                }

                return Result.Success(new ReadLongUrlResponse(cachedUrl.ToString()));
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("[CACHE MISS] no cached long url found for short URL: {ShortUrl}", shortUrl);
            }

            var getUrlResponse = await urlShortenerService.GetLongUrlAsync(shortUrl, cancellationToken);
            if (getUrlResponse.IsFailure)
            {
                return getUrlResponse;
            }

            await _redisDb.StringSetAsync(
                shortUrl,
                getUrlResponse.Value.LongUrl,
                TimeSpan.FromHours(1)
                ).WaitAsync(cancellationToken);

            return getUrlResponse;
        }
    }
}