using Microsoft.Azure.Cosmos;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Core.UseCases.Urls.List;
using System.Text;

namespace Shortix.UrlShortener.Infrastructure.Data.Repositories
{
    internal sealed class UserUrlsRepository(Container container) : IUserUrlsRepository
    {
        public async Task<ListUrlsByUserResponse> GetUrlsByCustomerAsync(string email, int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition("SELECT * FROM c  WHERE c.PartitionKey = @partitionKey")
                      .WithParameter("@partitionKey", email);

            var queryContinuationToken = continuationToken is null
                ? null
                : Encoding.UTF8.GetString(Convert.FromBase64String(continuationToken));

            var iterator = container.GetItemQueryIterator<ShortenedUrlCosmos>(query,
                continuationToken: queryContinuationToken,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(email),
                    MaxItemCount = pageSize
                });

            var results = new List<ShortenedUrlCosmos>();
            string? resultContinuationToken = null;
            var readItemsCount = 0;

            while (readItemsCount < pageSize && iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
                readItemsCount += response.Count;
                resultContinuationToken = response.ContinuationToken;
            }

            var responseContinuationToken = resultContinuationToken is null
                ? null
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(resultContinuationToken));

            return new ListUrlsByUserResponse(
                results.Select(e =>
                    new UserShortUrlItem(e.ShortUrl, e.LongUrl, e.CreatedOn))
                    .ToList(),
                responseContinuationToken);
        }
    }
}