using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Shortix.CosmosDbTriggerFunction.Configurations;

var builder = FunctionsApplication
    .CreateBuilder(args)
    .ConfigureFunctionsWebApplication()
    .AddKeyVault()
    .AddCosmosDb()
    .AddFunctionHealthChecks();

builder.Build().Run();