using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidR.DependencyInjection;
using Shortix.Commons.Core.Behaviors;
using Shortix.UrlShortener.Core;
using Shortix.UrlShortener.Core.Interfaces;
using Shortix.UrlShortener.Infrastructure.Data.Repositories;
using Shortix.UrlShortener.Infrastructure.Services;

namespace Shortix.UrlShortener.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCosmosDb(configuration);

            services.AddMidR(AssemblyReference.Assembly).WithBehaviors(behaviors =>
            {
                behaviors.AddBehavior(typeof(LoggingBehavior<,>)).WithPriority(1);
                behaviors.AddBehavior(typeof(ValidationBehavior<,>)).WithPriority(2);
            });

            services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

            services.AddSingleton<ITokenService, TokenService>();
            services.AddTransient<IUrlRepository, UrlRepository>();

            return services;
        }

        private static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<CosmosClient>(c =>
                new(configuration["CosmosDb:ConnectionString"]!));

            services.AddSingleton<IUrlRepository>(c =>
            {
                var cosmosClient = c.GetRequiredService<CosmosClient>();
                var container = cosmosClient.GetContainer(
                    configuration["CosmosDb:DatabaseName"]!,
                    configuration["CosmosDb:ContainerName"]!);

                return new UrlRepository(container);
            });

            return services;
        }
    }
}