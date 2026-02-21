using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.UseCases.Urls.List;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface IUserUrlsRepository
    {
        Task<ListUrlsByUserResponse> GetUrlsByCustomerAsync(string email, int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default);
    }
}