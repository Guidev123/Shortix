using Newtonsoft.Json;
using Shortix.UrlShortener.Core.Models;

namespace Shortix.UrlShortener.Core.DTOs
{
    public sealed class ShortenedUrlCosmos
    {
        public string LongUrl { get; }

        [JsonProperty(PropertyName = "id")]
        public string ShortUrl { get; }

        public DateTimeOffset CreatedOn { get; }
        public string CreatedBy { get; }

        public string PartitionKey => ShortUrl[..1];

        public ShortenedUrlCosmos(string longUrl, string shortUrl, string createdBy, DateTimeOffset createdOn)
        {
            LongUrl = longUrl;
            ShortUrl = shortUrl;
            CreatedOn = createdOn;
            CreatedBy = createdBy;
        }

        public static implicit operator ShortenedUrl(ShortenedUrlCosmos url) =>
            ShortenedUrl.Create(new Uri(url.LongUrl), url.ShortUrl, url.CreatedBy, url.CreatedOn);

        public static explicit operator ShortenedUrlCosmos(ShortenedUrl url) =>
            new(url.LongUrl.ToString(), url.ShortUrl, url.CreatedBy, url.CreatedOn);
    }
}