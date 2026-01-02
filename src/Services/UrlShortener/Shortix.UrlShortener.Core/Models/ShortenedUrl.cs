namespace Shortix.UrlShortener.Core.Models
{
    public sealed class ShortenedUrl
    {
        private ShortenedUrl(Uri longUrl, string shortUrl, string createdBy, DateTimeOffset createdOn)
        {
            LongUrl = longUrl;
            ShortUrl = shortUrl;
            CreatedBy = createdBy;
            CreatedOn = createdOn;
        }

        public Uri LongUrl { get; private set; }
        public string ShortUrl { get; private set; }
        public string CreatedBy { get; private set; }
        public DateTimeOffset CreatedOn { get; private set; }

        public static ShortenedUrl Create(Uri longUrl, string shortUrl, string createdBy, DateTimeOffset createdOn)
        {
            var shortenedUrl = new ShortenedUrl(longUrl, shortUrl, createdBy, createdOn);

            return shortenedUrl;
        }
    }
}