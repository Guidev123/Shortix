using Shortix.UrlShortener.Core.Models;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface IShortUrlRepository
    {
        Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default);
    }
}