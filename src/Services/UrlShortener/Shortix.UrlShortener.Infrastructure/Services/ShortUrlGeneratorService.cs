using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Extensions;
using Shortix.UrlShortener.Core.Interfaces;

namespace Shortix.UrlShortener.Infrastructure.Services
{
    internal sealed class ShortUrlGeneratorService(ITokenService tokenService) : IShortUrlGeneratorService
    {
        public async Task<Result<UniqueUrlResponse>> GenerateUniqueUrlAsync(CancellationToken cancellationToken = default)
        {
            var tokenResult = await tokenService.GetTokenAsync(cancellationToken);
            if (tokenResult.IsFailure)
            {
                return Result.Failure<UniqueUrlResponse>(tokenResult.Error!);
            }

            return new UniqueUrlResponse(tokenResult.Value.Token.EncodeToBase62());
        }
    }
}