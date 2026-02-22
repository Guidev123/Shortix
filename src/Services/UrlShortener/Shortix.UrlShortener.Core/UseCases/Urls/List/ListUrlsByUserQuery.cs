using Shortix.Commons.Core.Messaging;

namespace Shortix.UrlShortener.Core.UseCases.Urls.List
{
    public sealed record ListUrlsByUserQuery(
        string Author,
        int? PageSize = null,
        string? ContinuationToken = null
        ) : IQuery<ListUrlsByUserResponse>;
}