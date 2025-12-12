using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface IShortUrlGeneratorService
    {
        Task<Result<UniqueUrlResponse>> GenerateUniqueUrlAsync(CancellationToken cancellationToken = default);
    }
}