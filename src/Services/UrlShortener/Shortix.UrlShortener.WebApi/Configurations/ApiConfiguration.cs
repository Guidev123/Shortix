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

            builder.AddSwaggerConfig();

            builder.Services.AddEndpoints(typeof(ApiConfiguration).Assembly);

            builder.Services.AddInfrastructureModule(builder.Configuration, builder.Environment);

            return builder;
        }

        public static WebApplication UseApiConfiguration(this WebApplication app)
        {
            app.UseCommonPipeline().UseSwaggerConfig();

            return app;
        }
    }
}