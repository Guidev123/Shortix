using Newtonsoft.Json;

namespace Shortix.CosmosDbTriggerFunction.Models
{
    public sealed class ShortenedUrl
    {
        public string LongUrl { get; }

        [JsonProperty(PropertyName = "id")]
        public string ShortUrl { get; }

        public DateTimeOffset CreatedOn { get; }

        [JsonProperty(PropertyName = "PartitionKey")]
        public string CreatedBy { get; }

        public ShortenedUrl(string longUrl, string shortUrl,
            DateTimeOffset createdOn, string createdBy)
        {
            LongUrl = longUrl;
            ShortUrl = shortUrl;
            CreatedOn = createdOn;
            CreatedBy = createdBy;
        }
    }
}