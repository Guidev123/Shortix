using FluentAssertions;

namespace Shortix.UrlShortener.Core.Tests.GenerateShortUrl
{
    public class GenerateShortUrlScenarios
    {
        [Fact]
        public void Should_Return_ShortUrl_For_10001()
        {
            var tokenProvider = new TokenProvider();
            tokenProvider.AssignRange(10001, 20000);
            var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);

            var shortUrl = shortUrlGenerator.GenerateUniqueUrl();

            shortUrl.Should().Be("2bJ");
        }

        [Fact]
        public void Should_Return_ShortUrl_For_Zero()
        {
            var tokenProvider = new TokenProvider();
            tokenProvider.AssignRange(0, 10);
            var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);

            var shortUrl = shortUrlGenerator.GenerateUniqueUrl();

            shortUrl.Should().Be("0");
        }
    }
}