using Shortix.Commons.Core.Results;
using Shortix.Redirect.WebApi.Models;

namespace Shortix.Redirect.WebApi.Interfaces
{
    public interface IUrlShortenerService
    {
        Task<Result<ReadLongUrlResponse>> GetLongUrlAsync(string shortUrl, CancellationToken cancellationToken = default);
    }
}