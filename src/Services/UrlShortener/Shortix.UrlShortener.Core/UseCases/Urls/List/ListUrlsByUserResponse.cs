using Shortix.UrlShortener.Core.DTOs;

namespace Shortix.UrlShortener.Core.UseCases.Urls.List
{
    public sealed record ListUrlsByUserResponse(IReadOnlyCollection<UserShortUrlItem> Items, string? ResponseContinuationToken);
}