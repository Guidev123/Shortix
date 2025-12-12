using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.AddMidR(AssemblyReference.Assembly).WithBehaviors(behaviors =>
            {
                behaviors.AddBehavior(typeof(LoggingBehavior<,>)).WithPriority(1);
                behaviors.AddBehavior(typeof(ValidationBehavior<,>)).WithPriority(2);
            });
            services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

            services.AddTransient<IShortUrlGeneratorService, ShortUrlGeneratorService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IUrlRepository, UrlRepository>();

            return services;
        }
    }
}