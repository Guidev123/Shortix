using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Infrastructure;

namespace Shortix.UrlShortener.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
        {
            builder.AddCommonConfiguration();

            builder.Services.AddEndpoints(typeof(ApiConfiguration).Assembly);

            builder.Services.AddInfrastructureModule(builder.Configuration);

            return builder;
        }

        public static WebApplication UseApiConfiguration(this WebApplication app)
        {
            app.UseCommonPipeline();

            return app;
        }
    }
}