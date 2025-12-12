using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Interfaces;

namespace Shortix.UrlShortener.Infrastructure.Services
{
    internal sealed class TokenService : ITokenService
    {
        private TokenRangeRequest? _tokenRange;

        public Task<Result> AssignRangeAsync(long start, long end, CancellationToken cancellationToken = default)
        {
            _tokenRange = new TokenRangeRequest(start, end);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> AssignRangeAsync(TokenRangeRequest tokenRange, CancellationToken cancellationToken = default)
        {
            _tokenRange = tokenRange;

            return Task.FromResult(Result.Success());
        }

        public Task<Result<TokenRangeResponse>> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success(new TokenRangeResponse(_tokenRange.Start)));
        }
    }
}