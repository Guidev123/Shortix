using MidR.DependencyInjection;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.TokenRange.WebApi.Features.AssignTokenRange;
using System.Reflection;

namespace Shortix.TokenRange.WebApi.Configurations
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

                builder.AddInfrastructure();

                return builder;
            }

            public WebApplicationBuilder AddInfrastructure()
            {
                builder.Services.AddSingleton(new AssignTokenRangeService(
                    builder.Configuration["Postgres:ConnectionString"]!
                    ));

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
