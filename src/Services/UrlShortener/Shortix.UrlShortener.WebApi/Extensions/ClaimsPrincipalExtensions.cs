using System.Security.Authentication;
using System.Security.Claims;

namespace Shortix.UrlShortener.WebApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        private const string EmailClaim = "preferred_username";

        public static string GetUserEmail(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.FirstOrDefault(c => c.Type == EmailClaim)?.Value ??
                throw new AuthenticationException($"Missing '{EmailClaim}' claim");
        }
    }
}