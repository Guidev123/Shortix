using Microsoft.Azure.Cosmos;
using MidR.DependencyInjection;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.Redirect.WebApi.Configurations;
using Shortix.Redirect.WebApi.Interfaces;
using Shortix.Redirect.WebApi.Services;
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

                builder.AddInfrastructure();

                return builder;
            }

            public WebApplicationBuilder AddInfrastructure()
            {
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

                builder.Services.AddSingleton(c =>
                {
                    var client = c.GetRequiredService<CosmosClient>();

                    return client.GetContainer(
                        builder.Configuration["CosmosDb:DatabaseName"]!,
                        builder.Configuration["CosmosDb:ContainerName"]!);
                });

                builder.Services.AddSingleton<IUrlShortenerService, UrlShortenerService>();

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