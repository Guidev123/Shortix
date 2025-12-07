using Azure.Identity;

namespace Shortix.UrlShortener.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        private const string KeyVaultName = "KeyVaultName";

        public static WebApplicationBuilder AddApiConfiguration(this WebApplicationBuilder builder)
        {
            builder
                .AddKeyVault()
                .Services
                .AddOpenApi();

            return builder;
        }

        public static WebApplication UseApiConfiguration(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            return app;
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
    }
}