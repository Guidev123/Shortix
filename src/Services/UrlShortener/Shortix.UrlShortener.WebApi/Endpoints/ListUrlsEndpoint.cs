using Microsoft.AspNetCore.Mvc;
using MidR.Interfaces;
using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Core.UseCases.Urls.List;
using Shortix.UrlShortener.WebApi.Extensions;
using System.Security.Claims;

namespace Shortix.UrlShortener.WebApi.Endpoints
{
    internal sealed class ListUrlsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/urls", async (
                [FromQuery] int? pageSize,
                [FromQuery(Name = "continuation")] string? continuationToken,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new ListUrlsByUserQuery(
                    claimsPrincipal.GetUserEmail(),
                    pageSize,
                    continuationToken), cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            });
        }
    }
}