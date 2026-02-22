using Shortix.Redirect.WebApi.Configurations;

var builder = WebApplication
    .CreateBuilder(args)
    .AddApiConfiguration();

var app = builder
    .Build()
    .UseApiConfiguration();

app.Run();