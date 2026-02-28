using Azure.Monitor.OpenTelemetry.Exporter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MidR.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

            public WebApplicationBuilder AddTelemetry()
            {
                var telemetryConnectionString = builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"];
                var appName = builder.Environment.ApplicationName ?? "TokenRangeApi";

                builder.Logging.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(serviceName: appName));

                    options.IncludeFormattedMessage = true;

                    if (telemetryConnectionString is not null)
                        options.AddAzureMonitorLogExporter(o => { o.ConnectionString = telemetryConnectionString; });
                    else
                        options.AddConsoleExporter();
                });

                builder.Services.AddOpenTelemetry()
                    .ConfigureResource(resource =>
                         resource.AddService(serviceName: appName)
                         ).WithTracing(tracing =>
                         {
                             tracing.AddHttpClientInstrumentation();
                             tracing.AddSource("Azure.*");
                             tracing.AddAspNetCoreInstrumentation();

                             if (telemetryConnectionString is not null)
                             {
                                 tracing.AddAzureMonitorTraceExporter(o =>
                                 {
                                     o.ConnectionString = telemetryConnectionString;
                                 });
                             }
                             else
                             {
                                 tracing.AddConsoleExporter();
                             }
                         }).WithMetrics(metrics =>
                         {
                             metrics
                                .AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation();

                             if (telemetryConnectionString is not null)
                             {
                                 metrics.AddAzureMonitorMetricExporter(o =>
                                 {
                                     o.ConnectionString = telemetryConnectionString;
                                 });
                             }
                             else
                             {
                                 metrics.AddConsoleExporter();
                             }
                         });

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