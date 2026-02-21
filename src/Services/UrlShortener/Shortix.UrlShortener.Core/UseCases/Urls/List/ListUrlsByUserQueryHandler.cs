using Shortix.Commons.Core.Messaging;
using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.Interfaces;

namespace Shortix.UrlShortener.Core.UseCases.Urls.List
{
    internal sealed class ListUrlsByUserQueryHandler(IUserUrlsRepository userUrlsRepository) : IQueryHandler<ListUrlsByUserQuery, ListUrlsByUserResponse>
    {
        private const int MaxPageSize = 25;

        public async Task<Result<ListUrlsByUserResponse>> ExecuteAsync(ListUrlsByUserQuery request, CancellationToken cancellationToken = default)
        {
            var pageSize = int.Min(request.PageSize ?? MaxPageSize, MaxPageSize);

            var urls = await userUrlsRepository.GetUrlsByCustomerAsync(
                request.Author,
                pageSize,
                request.ContinuationToken,
                cancellationToken
                );

            return urls;
        }
    }
}