namespace Shortix.UrlShortener.Core.DTOs
{
    public sealed record TokenRangeRequest
    {
        public TokenRangeRequest(long start, long end)
        {
            if (end < start)
            {
                throw new ArgumentException("Start token cannot be greater than end token.", nameof(start));
            }

            Start = start;
            End = end;
        }

        public long Start { get; }
        public long End { get; }
    }
}