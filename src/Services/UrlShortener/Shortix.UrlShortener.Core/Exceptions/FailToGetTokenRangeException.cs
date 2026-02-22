using Shortix.Commons.Core.Exceptions;
using Shortix.Commons.Core.Results;

namespace Shortix.UrlShortener.Core.Exceptions
{
    public sealed class FailToGetTokenRangeException : ShortixException
    {
        public FailToGetTokenRangeException(string requestName, Error? error = null, Exception? innerException = null) : base(requestName, error, innerException)
        {
        }
    }
}