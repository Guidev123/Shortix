using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface ITokenService
    {
        Task<Result> AssignRangeAsync(long start, long end, CancellationToken cancellationToken = default);

        Task<Result> AssignRangeAsync(TokenRangeRequest tokenRange, CancellationToken cancellationToken = default);

        Task<Result<TokenRangeResponse>> GetTokenAsync(CancellationToken cancellationToken = default);
    }
}