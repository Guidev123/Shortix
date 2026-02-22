namespace Shortix.UrlShortener.Core.UseCases.Urls.Add
{
    public sealed record AddUrlResponse(string ShortenedUrlId, Uri LongUrl);
}