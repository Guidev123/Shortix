using Shortix.UrlShortener.WebApi.Configurations;

var builder = WebApplication
    .CreateBuilder(args)
    .AddApiConfiguration();

var app = builder
    .Build()
    .UseApiConfiguration();

app.Run();