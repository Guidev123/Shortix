using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.Redirect.WebApi.Interfaces;
using Shortix.Redirect.WebApi.Telemetry;

namespace Shortix.Redirect.WebApi.Endpoints
{
    internal sealed class RedirectEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("r/{shortUrl}", async (string shortUrl,
                                                                   IUrlShortenerService urlShortenerService,
                                                                   CancellationToken cancellationToken) =>
            {
                var result = await urlShortenerService.GetLongUrlAsync(shortUrl, cancellationToken);

                if (result.IsSuccess)
                {
                    ApplicationDiagnostics.RedirectsCounter.Add(1);
                }

                return result.Match(success => Results.Redirect(success.LongUrl, true), ApiResults.Problem);
            });
        }
    }
}