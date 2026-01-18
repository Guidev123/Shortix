using Microsoft.AspNetCore.Mvc;
using MidR.Interfaces;
using Shortix.Commons.Core.Results;
using Shortix.Commons.Infrastructure.Endpoints;
using Shortix.Commons.Infrastructure.Extensions;

namespace Shortix.TokenRange.WebApi.Features.AssignTokenRange
{
    internal sealed class AssignTokenRangeEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/token-ranges/assign", async ([FromBody] AssignTokenRangeCommand command,
                                                                    [FromServices] ISender sender,
                                                                    CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(successValue => Results.Ok(successValue), ApiResults.Problem);
            });
        }
    }
}
