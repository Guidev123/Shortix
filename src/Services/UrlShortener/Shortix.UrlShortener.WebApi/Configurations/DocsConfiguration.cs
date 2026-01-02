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
                });

                return builder;
            }
        }

        extension(WebApplication app)
        {
            public WebApplication UseSwaggerConfig()
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

                return app;
            }
        }
    }
}