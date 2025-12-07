using Microsoft.AspNetCore.Http.HttpResults;
using Shortix.UrlShortener.WebApi.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("healthz", () =>
{
    return Results.Ok("Health!");
});

app.Run();