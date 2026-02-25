using HealthChecks.CosmosDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
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