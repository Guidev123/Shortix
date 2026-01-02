using MidR.Interfaces;
using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Core.UseCases.Urls.Add;

namespace Shortix.UrlShortener.WebApi.Endpoints
{
    internal sealed class AddUrlEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/urls", async (AddUrlCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(command.SetCreatedBy("guirafaelrn@gmail.com"), cancellationToken);

                return result.Match(apiResult => Results.Created($"/api/v1/urls/{apiResult.ShortenedUrl}", apiResult.ShortenedUrl), ApiResults.Problem);
            });
        }
    }
}