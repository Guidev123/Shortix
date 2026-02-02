using Shortix.Commons.Core.Results;

namespace Shortix.UrlShortener.Core.Errors
{
    public static class TokenRangeErrors
    {
        public static Error FailToGetTokenRange(string machineIdentifier) => Error.Problem(
            code: "TokenRangeErrors.FailToGetTokenRange",
            description: $"The system failed to get a token range for the machine with identifier '{machineIdentifier}'.");
    }
}