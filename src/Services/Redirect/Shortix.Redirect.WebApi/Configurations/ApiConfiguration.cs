using HealthChecks.CosmosDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MidR.DependencyInjection;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.Redirect.WebApi.Interfaces;
using Shortix.Redirect.WebApi.Services;
using StackExchange.Redis;
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

                builder.AddApplicationHealthChecks();

                builder.Services.AddMidR(Assembly.GetExecutingAssembly());

                builder.AddSwaggerConfig();

                builder.Services.AddEndpoints(typeof(ApiConfiguration).Assembly);

                builder.AddInfrastructure();

                return builder;
            }

            public WebApplicationBuilder AddInfrastructure()
            {
                builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

                builder.Services.AddSingleton(_ =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        var cosmosClientOptions = new CosmosClientOptions
                        {
                            ConnectionMode = ConnectionMode.Gateway,
                            LimitToEndpoint = true
                        };

                        var httpClientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };

                        cosmosClientOptions.HttpClientFactory = () =>
                            new HttpClient(httpClientHandler);

                        return new CosmosClient(builder.Configuration["CosmosDb:ConnectionString"]!, cosmosClientOptions);
                    }

                    return new CosmosClient(builder.Configuration["CosmosDb:ConnectionString"]!);
                });

                builder.Services.AddSingleton<IUrlShortenerService>(c =>
                {
                    var client = c.GetRequiredService<CosmosClient>();

                    var container = client.GetContainer(
                        builder.Configuration["CosmosDb:DatabaseName"]!,
                        builder.Configuration["CosmosDb:ContainerName"]!);

                    return new RedisUrlShortenerService(
                        c.GetRequiredService<ILogger<RedisUrlShortenerService>>(),
                        new UrlShortenerService(container),
                        c.GetRequiredService<IConnectionMultiplexer>()
                    );
                });

                return builder;
            }

            public WebApplicationBuilder AddApplicationHealthChecks()
            {
                builder.Services.AddHealthChecks()
                    .AddAzureCosmosDB(optionsFactory: _ => new AzureCosmosDbHealthCheckOptions()
                    {
                        DatabaseId = builder.Configuration["CosmosDb:DatabaseName"]!,
                    })
                    .AddRedis(provider =>
                    {
                        return provider.GetRequiredService<IConnectionMultiplexer>();
                    }, failureStatus: HealthStatus.Degraded);

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