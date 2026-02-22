using MidR.Interfaces;
using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Core.UseCases.Urls.Add;
using Shortix.UrlShortener.WebApi.Extensions;
using System.Security.Claims;

namespace Shortix.UrlShortener.WebApi.Endpoints
{
    internal sealed class AddUrlEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/urls", async (AddUrlCommand command, ISender sender, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(command.SetCreatedBy(claimsPrincipal.GetUserEmail()), cancellationToken);

                return result.Match(apiResult => Results.Created($"/api/v1/urls/{apiResult.ShortenedUrlId}", apiResult), ApiResults.Problem);
            });
        }
    }
}