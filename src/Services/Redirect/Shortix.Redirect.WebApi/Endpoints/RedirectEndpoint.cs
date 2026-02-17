using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.Redirect.WebApi.Interfaces;

namespace Shortix.Redirect.WebApi.Endpoints
{
    internal sealed class RedirectEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/redirect/{shortUrl}", async (string shortUrl,
                                                                   IUrlShortenerService urlShortenerService,
                                                                   CancellationToken cancellationToken) =>
            {
                var result = await urlShortenerService.GetLongUrlAsync(shortUrl, cancellationToken);
                return result.Match(success => Results.Redirect(success.LongUrl, true), ApiResults.Problem);
            });
        }
    }
}