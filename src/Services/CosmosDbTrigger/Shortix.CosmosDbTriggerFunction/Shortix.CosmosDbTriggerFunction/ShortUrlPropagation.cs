using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shortix.CosmosDbTriggerFunction.Models;

namespace Shortix.CosmosDbTriggerFunction
{
    public class ShortUrlPropagation(ILoggerFactory loggerFactory, Container container)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ShortUrlPropagation>();

        [Function("ShortUrlPropagation")]
        public async Task Run([CosmosDBTrigger(
                databaseName: "urls",
                containerName: "items",
                Connection = "CosmosDbConnection",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<UrlDocument> urlDocuments)
        {
            if (urlDocuments is null || urlDocuments.Count <= 0) return;

            foreach (var url in urlDocuments)
            {
                try
                {
                    var cosmosDbDocument = new ShortenedUrl(
                        url.LongUrl,
                        url.Id,
                        url.CreatedOn,
                        url.CreatedBy
                    );

                    await container.UpsertItemAsync(cosmosDbDocument, new PartitionKey(cosmosDbDocument.CreatedBy));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error upserting document with Short Url Identifier: {ShortUrl} | Created by: {CreatedBy}", url.Id, url.CreatedBy);
                    throw;
                }
            }
        }
    }
}