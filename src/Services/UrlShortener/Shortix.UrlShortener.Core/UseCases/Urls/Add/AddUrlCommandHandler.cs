using Shortix.Commons.Core.Messaging;
using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Core.Models;

namespace Shortix.UrlShortener.Core.UseCases.Urls.Add
{
    internal sealed class AddUrlCommandHandler(
        IShortUrlGeneratorService shortUrlGeneratorService,
        IUrlRepository urlRepository,
        TimeProvider timeProvider) : ICommandHandler<AddUrlCommand, AddUrlResponse>
    {
        public async Task<Result<AddUrlResponse>> ExecuteAsync(AddUrlCommand request, CancellationToken cancellationToken = default)
        {
            var uniqueUrlResult = await shortUrlGeneratorService.GenerateUniqueUrlAsync(cancellationToken);

            var longUrlUriFormat = new Uri(request.LongUrl);

            var shortenedUrl = ShortenedUrl.Create(longUrlUriFormat, uniqueUrlResult.Value.UniqueUrl, request.CreatedBy, timeProvider.GetUtcNow());

            await urlRepository.AddAsync(shortenedUrl, cancellationToken);

            return new AddUrlResponse(uniqueUrlResult.Value.UniqueUrl, longUrlUriFormat);
        }
    }
}