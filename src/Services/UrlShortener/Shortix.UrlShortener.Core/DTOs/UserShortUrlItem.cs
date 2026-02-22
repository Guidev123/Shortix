namespace Shortix.UrlShortener.Core.DTOs
{
    public sealed record UserShortUrlItem(
        string ShortUrlId,
        string LongUrl,
        DateTimeOffset CreatedOn
        );
}