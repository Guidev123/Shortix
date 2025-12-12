using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shortix.Commons.Infrastructure.Extensions;
using Shortix.UrlShortener.Core.UseCases.Urls.Add;
using System.Net.Http.Json;

namespace UrlShortener.Tests
{
    public class AddUrlFeature : IClassFixture<ApiFixture>
    {
        private readonly HttpClient _client;

        public AddUrlFeature(ApiFixture apiFixture)
        {
            _client = apiFixture.CreateClient();
        }

        [Fact]
        public async Task Given_LongUrl_ShouldReturn_ShortUrl()
        {
            // Arrange & Act
            var result = await _client.PostAsJsonAsync("/api/v1/urls", CreateCommand());

            var addUrlResponse = result.Content.ReadFromJsonAsync<AddUrlResponse>();

            // Assert
            result.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            addUrlResponse.Should().NotBeNull();
        }

        private static AddUrlCommand CreateCommand() =>
            new AddUrlCommand("https://www.example.com/some/long/url").SetCreatedBy("test-user");
    }

    public class ApiFixture : WebApplicationFactory<Program>
    {
    }
}