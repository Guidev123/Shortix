using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Core.Models;

namespace Shortix.UrlShortener.Infrastructure.Data.Repositories
{
    internal sealed class UrlRepository : IUrlRepository
    {
        public Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}