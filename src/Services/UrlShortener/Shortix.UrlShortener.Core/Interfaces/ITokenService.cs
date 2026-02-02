using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Events;

namespace Shortix.UrlShortener.Core.Interfaces
{
    public interface ITokenService
    {
        Result<long> GetToken();

        void AssignRange(TokenRangeRequest tokenRange);

        void AssignRange(int start, int end);

        event EventHandler<ReachingRangeLimitEventArgs>? ReachingRangeLimit;
    }
}