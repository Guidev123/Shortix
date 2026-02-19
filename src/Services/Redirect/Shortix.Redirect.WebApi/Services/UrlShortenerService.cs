using Microsoft.Azure.Cosmos;
using Shortix.Commons.Core.Results;
using Shortix.Redirect.WebApi.Errors;
using Shortix.Redirect.WebApi.Interfaces;
using Shortix.Redirect.WebApi.Models;
using System.Net;

namespace Shortix.Redirect.WebApi.Services
{
    public sealed class UrlShortenerService(Container container) : IUrlShortenerService
    {
        public async Task<Result<ReadLongUrlResponse>> GetLongUrlAsync(string shortUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var record = await container.ReadItemAsync<CosmosLongUrlResponse>(shortUrl, new PartitionKey(shortUrl[..1]), cancellationToken: cancellationToken);

                return record switch
                {
                    { Resource: not null } => new ReadLongUrlResponse(record.Resource.LongUrl),
                    _ => Result.Failure<ReadLongUrlResponse>(RedirectErrors.ShortUrlNotFound)
                };
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<ReadLongUrlResponse>(RedirectErrors.ShortUrlNotFound);
            }
        }
    }
}