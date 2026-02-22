using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        }

        extension(WebApplication app)
        {
            public WebApplication UseApiConfiguration()
            {
                app.UseSwaggerConfig();

                app.UseCors(CorsPolicyName);

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseCommonPipeline();

                return app;
            }
        }
    }
}