namespace Shortix.UrlShortener.Core.UseCases.Urls.Add
{
    public sealed record AddUrlResponse(string ShortenedUrl, Uri LongUrl);
}