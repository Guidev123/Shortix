using Shortix.UrlShortener.Core.Extensions;

namespace Shortix.UrlShortener.Core
{
    public class ShortUrlGenerator(TokenProvider tokenProvider)
    {
        public string GenerateUniqueUrl()
        {
            return tokenProvider.GetToken().EncodeToBase62();
        }
    }
}