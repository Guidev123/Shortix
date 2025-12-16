using Microsoft.Azure.Cosmos;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Core.Models;

namespace Shortix.UrlShortener.Infrastructure.Data.Repositories
{
    internal sealed class UrlRepository(Container container) : IUrlRepository
    {
        public async Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default)
        {
            var document = (ShortenedUrlCosmos)shortenedUrl;

            await container.CreateItemAsync(document, new PartitionKey(document.PartitionKey), cancellationToken: cancellationToken);
        }
    }
}