using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MidR.DependencyInjection;
using Shortix.Commons.Core.Behaviors;
using Shortix.UrlShortener.Core;
using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Infrastructure.BackgroundServices;
using Shortix.UrlShortener.Infrastructure.Data.Repositories;
using Shortix.UrlShortener.Infrastructure.ExternalServices;
using Shortix.UrlShortener.Infrastructure.Services;

namespace Shortix.UrlShortener.Infrastructure
{
    public static class InfrastructureModule
    {
        public const string TokenRangesHttpClientName = "TokenRanges";

        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddCosmosDb(configuration, environment);

            services.AddMidR(AssemblyReference.Assembly).WithBehaviors(behaviors =>
            {
                behaviors.AddBehavior(typeof(LoggingBehavior<,>)).WithPriority(1);
                behaviors.AddBehavior(typeof(ValidationBehavior<,>)).WithPriority(2);
            });

            services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IEnvironmentManager, EnvironmentManager>();

            services.AddHostedService<TokenRangeManager>();

            services.AddHttpClient(TokenRangesHttpClientName, client =>
            {
                client.BaseAddress = new Uri(configuration["TokenRangeService:BaseUrl"]!);
            });

            services.AddSingleton<ITokenRangeApiService, TokenRangeApiService>();

            return services;
        }

        private static IServiceCollection AddCosmosDb(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddSingleton(_ =>
            {
                if (environment.IsDevelopment())
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

                    return new CosmosClient(configuration["CosmosDb:ConnectionString"]!, cosmosClientOptions);
                }

                return new CosmosClient(configuration["CosmosDb:ConnectionString"]!);
            });

            services.AddSingleton<IShortUrlRepository>(c =>
            {
                var client = c.GetRequiredService<CosmosClient>();

                var container = client.GetContainer(
                    configuration["CosmosDb:DatabaseName"]!,
                    configuration["CosmosDb:ContainerName"]!);

                return new ShortUrlRepository(container);
            });

            services.AddSingleton<IUserUrlsRepository>(c =>
            {
                var client = c.GetRequiredService<CosmosClient>();

                var container = client.GetContainer(
                    configuration["CosmosDb:ByUserDatabaseName"]!,
                    configuration["CosmosDb:ByUserContainerName"]!);

                return new UserUrlsRepository(container);
            });

            return services;
        }
    }
}