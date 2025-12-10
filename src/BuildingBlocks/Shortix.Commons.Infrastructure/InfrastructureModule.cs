using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shortix.Commons.Infrastructure.Middlewares;
using Serilog;
using Microsoft.Extensions.Configuration;
using Azure.Identity;

namespace Shortix.Commons.Infrastructure
{
    public static class InfrastructureModule
    {
        private const string KeyVaultName = "KeyVaultName";

        public static WebApplicationBuilder AddCommonConfiguration(this WebApplicationBuilder builder)
        {
            builder.AddExceptionHandler();

            builder.Services.AddOpenApi();

            builder.Host.UseSerilog((context, loggerConfig)
                => loggerConfig.ReadFrom.Configuration(context.Configuration));

            builder.AddKeyVault();

            return builder;
        }

        private static WebApplicationBuilder AddExceptionHandler(this WebApplicationBuilder builder)
        {
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            return builder;
        }

        private static WebApplicationBuilder AddKeyVault(this WebApplicationBuilder builder)
        {
            var keyVaultName = builder.Configuration[KeyVaultName];
            if (!string.IsNullOrEmpty(keyVaultName))
            {
                builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultName}.vault.azure.net/"),
                new DefaultAzureCredential());
            }

            return builder;
        }

        public static WebApplication UseCommonPipeline(this WebApplication app)
        {
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSerilogRequestLogging();
                app.LogContextTraceLoggingMiddleware();
            }

            return app;
        }

        private static IApplicationBuilder LogContextTraceLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogContextTraceLoggingMiddleware>();

            return app;
        }
    }
}