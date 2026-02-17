using MidR.DependencyInjection;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.Redirect.WebApi.Configurations;
using Shortix.TokenRange.WebApi.Configurations;
using System.Reflection;

namespace Shortix.Redirect.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        extension(WebApplicationBuilder builder)
        {
            public WebApplicationBuilder AddApiConfiguration()
            {
                builder.AddCommonConfiguration();

                builder.Services.AddMidR(Assembly.GetExecutingAssembly());

                builder.AddSwaggerConfig();

                builder.Services.AddEndpoints(typeof(ApiConfiguration).Assembly);

                return builder;
            }
        }

        extension(WebApplication app)
        {
            public WebApplication UseApiConfiguration()
            {
                app.UseCommonPipeline().UseSwaggerConfig();

                return app;
            }
        }
    }
}