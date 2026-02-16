using Microsoft.OpenApi;

namespace Shortix.UrlShortener.WebApi.Configurations
{
    public static class DocsConfiguration
    {
        extension(WebApplicationBuilder builder)
        {
            public WebApplicationBuilder AddSwaggerConfig()
            {
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo()
                    {
                        Title = "Url Shortener",
                        Contact = new OpenApiContact() { Name = "Guilherme Nascimento", Email = "guirafaelrn@gmail.com" },
                        License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/license/MIT") }
                    });

                    var clientId = builder.Configuration["AzureAd:ClientId"];
                    var tenantId = builder.Configuration["AzureAd:TenantId"];
                    var scope = builder.Configuration["AzureAd:Scopes"];
                    var scopeUri = $"api://{clientId}/{scope}";

                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                                Scopes = new Dictionary<string, string>
                                {
                                { scopeUri, "Access API" }
                                }
                            }
                        }
                    });

                    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", document)] = [scopeUri]
                    });
                });

                return builder;
            }
        }

        extension(WebApplication app)
        {
            public WebApplication UseSwaggerConfig()
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    c.OAuthClientId(app.Configuration["AzureAd:ClientId"]);
                    c.OAuthUsePkce();
                    c.OAuthScopeSeparator(" ");
                    c.OAuthScopes($"api://{app.Configuration["AzureAd:ClientId"]}/{app.Configuration["AzureAd:Scopes"]}");
                });

                return app;
            }
        }
    }
}