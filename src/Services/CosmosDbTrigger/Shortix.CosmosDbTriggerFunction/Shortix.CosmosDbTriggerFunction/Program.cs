using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Shortix.CosmosDbTriggerFunction.Configurations;

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication()
    .AddKeyVault()
    .AddCosmosDb();

builder.Build().Run();