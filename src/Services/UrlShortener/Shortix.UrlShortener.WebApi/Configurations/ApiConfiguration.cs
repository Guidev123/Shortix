using Azure.Identity;

namespace Shortix.UrlShortener.WebApi.Configurations
{
    public static class ApiConfiguration
    {
        private const string KeyVaultName = "KeyVaultName";

        public static void AddApiConfiguration(this WebApplicationBuilder builder)
            => builder
                .AddKeyVault()
                .Services
                .AddOpenApi();

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