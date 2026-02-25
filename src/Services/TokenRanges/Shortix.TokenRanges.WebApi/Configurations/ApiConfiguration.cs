using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MidR.DependencyInjection;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.TokenRanges.WebApi.Configurations;
using Shortix.TokenRanges.WebApi.Features.AssignTokenRange;
using System.Reflection;

namespace Shortix.TokenRanges.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        extension(WebApplicationBuilder builder)
        {
            public WebApplicationBuilder AddApiConfiguration()
            {
                builder.AddCommonConfiguration();

                builder.AddApplicationHealthChecks();

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

            public WebApplicationBuilder AddApplicationHealthChecks()
            {
                builder.Services.AddHealthChecks()
                    .AddNpgSql(builder.Configuration["Postgres:ConnectionString"]!);

                return builder;
            }
        }

        extension(WebApplication app)
        {
            public WebApplication UseApiConfiguration()
            {
                app.UseCommonPipeline();

                app.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                app.UseSwaggerConfig();

                return app;
            }
        }
    }
}