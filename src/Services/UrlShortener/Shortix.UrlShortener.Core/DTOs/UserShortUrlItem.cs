namespace Shortix.UrlShortener.Core.DTOs
{
    public sealed record UserShortUrlItem(
        string ShortUrl,
        string LongUrl,
        DateTimeOffset CreatedOn
        );
}