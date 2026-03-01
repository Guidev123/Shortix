using Azure.Identity;
using HealthChecks.CosmosDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shortix.CosmosDbTriggerFunction.Configurations
{
    public static class FunctionConfiguration
    {
        private const string KeyVaultName = "KeyVaultName";

        public static FunctionsApplicationBuilder AddAppInsights(this FunctionsApplicationBuilder builder)
        {
            if (builder.Environment.IsProduction())
            {
                builder
                    .Services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights();
            }

            return builder;
        }

        public static FunctionsApplicationBuilder AddKeyVault(this FunctionsApplicationBuilder builder)
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

        public static FunctionsApplicationBuilder AddCosmosDb(this FunctionsApplicationBuilder builder)
        {
            builder.Services.AddSingleton(_ =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    var cosmosClientOptions = new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Gateway,
                        LimitToEndpoint = true
                    };

                    var httpClientHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    cosmosClientOptions.HttpClientFactory = () =>
                        new HttpClient(httpClientHandler);

                    return new CosmosClient(builder.Configuration["CosmosDbConnection"]!, cosmosClientOptions);
                }

                return new CosmosClient(builder.Configuration["CosmosDbConnection"]!);
            });

            builder.Services.AddSingleton(c =>
            {
                var client = c.GetRequiredService<CosmosClient>();

                return client.GetContainer(
                    builder.Configuration["TargetDatabaseName"]!,
                    builder.Configuration["TargetContainerName"]!);
            });

            return builder;
        }

        public static FunctionsApplicationBuilder AddFunctionHealthChecks(this FunctionsApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                .AddAzureCosmosDB(optionsFactory: _ => new AzureCosmosDbHealthCheckOptions()
                {
                    DatabaseId = builder.Configuration["TargetDatabaseName"]!,
                });
            return builder;
        }
    }
}