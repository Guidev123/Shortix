namespace Shortix.UrlShortener.Core.Events
{
    public sealed class ReachingRangeLimitEventArgs : EventArgs
    {
        public long Token { get; set; }
        public long RangeLimit { get; set; }
    }
}