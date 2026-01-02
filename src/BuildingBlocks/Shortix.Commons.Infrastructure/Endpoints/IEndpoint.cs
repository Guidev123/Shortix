using Microsoft.AspNetCore.Routing;

namespace Shortix.Commons.Infrastructure.Endpoints
{
    public interface IEndpoint
    {
        void MapEndpoint(IEndpointRouteBuilder app);
    }
}