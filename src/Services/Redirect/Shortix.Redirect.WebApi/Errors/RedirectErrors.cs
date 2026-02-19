using Shortix.Commons.Core.Results;

namespace Shortix.Redirect.WebApi.Errors
{
    public static class RedirectErrors
    {
        public static readonly Error ShortUrlNotFound = Error.NotFound(
            "Redirect.ShortUrlNotFound",
            "Short URL not found");
    }
}