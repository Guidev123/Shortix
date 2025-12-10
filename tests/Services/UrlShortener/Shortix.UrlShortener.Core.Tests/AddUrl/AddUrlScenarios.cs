using FluentAssertions;

namespace Shortix.UrlShortener.Core.Tests.AddUrl
{
    public class AddUrlScenarios
    {
        [Fact]
        public async Task Should_Return_ShortenedUrl()
        {
            var tokenProvider = new TokenProvider();
            var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);
            tokenProvider.AssignRange(1, 5);

            var handler = new AddUrlHandler(shortUrlGenerator);
            var command = new AddUrlCommand(new Uri("https://test.com"));
            var response = await handler.ExecuteAsync(command, CancellationToken.None);

            response.ShortenedUrl.Should().NotBeEmpty();
            response.ShortenedUrl.Should().Be("1");
        }
    }

    public record AddUrlCommand(Uri LongUrl);

    public record AddUrlResponse(string ShortenedUrl, Uri LongUrl);

    internal class AddUrlHandler(ShortUrlGenerator shortUrlGenerator)
    {
        public async Task<AddUrlResponse> ExecuteAsync(AddUrlCommand command, CancellationToken none)
        {
            return new AddUrlResponse(shortUrlGenerator.GenerateUniqueUrl(), command.LongUrl);
        }
    }
}