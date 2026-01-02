using Shortix.Commons.Core.Results;

namespace Shortix.UrlShortener.Core.Errors
{
    public static class ShortenedUrlErrors
    {
        public static readonly Error CreatedByShouldBeNotEmpty = Error.Problem(
            "ShortenedUrl.CreatedByShouldBeNotEmpty",
            "CreatedBy is required");

        public static readonly Error UrlShouldNotBeEmpty = Error.Problem(
            "ShortenedUrl.UrlShouldNotBeEmpty",
            "URL must not be empty");

        public static Error UrlMaxLengthExceeded(int maxLength) =>
            Error.Problem(
                "ShortenedUrl.UrlMaxLengthExceeded",
                $"URL must have a maximum length of {maxLength} characters");

        public static readonly Error UrlShouldBeAbsolute = Error.Problem(
            "ShortenedUrl.UrlShouldBeAbsolute",
            "URL must be a valid absolute address with HTTP or HTTPS scheme");
    }
}