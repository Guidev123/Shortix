using Shortix.Commons.Core.Results;
using Shortix.UrlShortener.Core.DTOs;
using Shortix.UrlShortener.Core.Interfaces;
using System.Collections.Concurrent;

namespace Shortix.UrlShortener.Infrastructure.Services
{
    internal sealed class TokenService : ITokenService
    {
        private readonly Lock _tokenLock = new();
        private readonly ConcurrentQueue<TokenRangeRequest> _ranges = new();

        private long _currentToken = 0;
        private TokenRangeRequest? _currentTokenRange;

        public void AssignRange(int start, int end)
        {
            AssignRange(new TokenRangeRequest(start, end));
        }

        public void AssignRange(TokenRangeRequest tokenRange)
        {
            _ranges.Enqueue(tokenRange);
        }

        public Result<long> GetToken()
        {
            lock (_tokenLock)
            {
                if (_currentTokenRange is null)
                    MoveToNextRange();

                if (_currentToken > _currentTokenRange?.End)
                    MoveToNextRange();

                if (IsReachingRangeLimit())
                    OnRangeThresholdReached(new ReachingRangeLimitEventArgs()
                    {
                        RangeLimit = _currentTokenRange!.End,
                        Token = _currentToken
                    });

                return _currentToken++;
            }
        }

        private bool IsReachingRangeLimit()
        {
            var currentIndex = _currentToken + 1 - _currentTokenRange!.Start;
            var total = _currentTokenRange.End - _currentTokenRange.Start;
            return currentIndex >= total * 0.8;
        }

        private event EventHandler? ReachingRangeLimit;

        private void OnRangeThresholdReached(EventArgs e)
        {
            ReachingRangeLimit?.Invoke(this, e);
        }

        private void MoveToNextRange()
        {
            if (!_ranges.TryDequeue(out _currentTokenRange))
                throw new IndexOutOfRangeException();

            _currentToken = _currentTokenRange.Start;
        }
    }

    internal sealed class ReachingRangeLimitEventArgs : EventArgs
    {
        public long Token { get; set; }
        public long RangeLimit { get; set; }
    }
}