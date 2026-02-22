using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface ITokenRangeApiService
    {
        Task<Result<TokenRangeApiResponse>> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken = default);
    }
}