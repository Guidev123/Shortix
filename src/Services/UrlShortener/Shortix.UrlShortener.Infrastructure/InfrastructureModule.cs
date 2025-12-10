using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidR.DependencyInjection;
using Shortix.Commons.Core.Behaviors;
using Shortix.UrlShortener.Core;

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

            return services;
        }
    }
}