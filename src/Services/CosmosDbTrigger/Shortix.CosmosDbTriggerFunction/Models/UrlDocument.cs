namespace Shortix.CosmosDbTriggerFunction.Models
{
    public sealed class UrlDocument
    {
        public string Id { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTimeOffset CreatedOn { get; set; }

        public string LongUrl { get; set; } = string.Empty;
    }
}