using Azure.Monitor.OpenTelemetry.Exporter;
using HealthChecks.CosmosDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shortix.Commons.Infrastructure;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Infrastructure;

namespace Shortix.UrlShortener.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        private const string CorsPolicyName = "AllowWebApp";

        extension(WebApplicationBuilder builder)
        {
            public WebApplicationBuilder AddApiConfiguration()
            {
                builder.AddSwaggerConfig();

                builder.AddCommonConfiguration();

                builder.AddApplicationHealthChecks();

                builder.AddAuthenticationWithAzureEntraId();
                builder.AddAuthorizationWithAzureEntraId();

                builder.Services.AddEndpoints(typeof(ApiConfiguration).Assembly);

                builder.Services.AddInfrastructureModule(builder.Configuration, builder.Environment);

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(CorsPolicyName, policy =>
                    {
                        if (builder.Configuration["WebAppEndpoints"] is null) return;

                        var origins = builder.Configuration["WebAppEndpoints"]!.Split(',');

                        policy
                            .WithOrigins([.. origins])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
                });

                builder.AddTelemetry();

                return builder;
            }

            private void AddAuthenticationWithAzureEntraId()
            {
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(options =>
                    {
                        builder.Configuration.Bind("AzureAd", options);
                        options.TokenValidationParameters.NameClaimType = "name";
                    }, options =>
                    {
                        builder.Configuration.Bind("AzureAd", options);
                    });
            }

            private void AddAuthorizationWithAzureEntraId()
            {
                builder.Services.AddAuthorizationBuilder()
                    .AddPolicy("AuthZPolicy", policyBuilder =>
                    {
                        policyBuilder.Requirements.Add(new ScopeAuthorizationRequirement()
                        {
                            RequiredScopesConfigurationKey = "AzureAd:Scopes"
                        });
                    });

                builder.Services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(
                        JwtBearerDefaults.AuthenticationScheme
                        ).RequireAuthenticatedUser()
                        .Build();

                    options.FallbackPolicy = options.DefaultPolicy;
                });
            }

            public WebApplicationBuilder AddApplicationHealthChecks()
            {
                builder.Services.AddHealthChecks()
                    .AddAzureCosmosDB(optionsFactory: _ => new AzureCosmosDbHealthCheckOptions()
                    {
                        DatabaseId = builder.Configuration["CosmosDb:DatabaseName"]!,
                    })
                    .AddUrlGroup(new Uri(
                        new Uri(
                            builder.Configuration["TokenRangeService:BaseUrl"]!),
                        "healthz"),
                        name: "Token Range Service"
                        );

                return builder;
            }

            public WebApplicationBuilder AddTelemetry()
            {
                var telemetryConnectionString = builder.Configuration["APPINSIGHTS_CONNECTIONSTRING"];
                var appName = builder.Environment.ApplicationName ?? "UrlShortenerApi";

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
                             tracing.AddSource("Azure.Cosmos.Operation");
                             tracing.AddRedisInstrumentation();
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

                app.UseCors(CorsPolicyName);

                app.UseAuthentication();
                app.UseAuthorization();

                return app;
            }
        }
    }
}