using Shortix.Commons.Core.Results;

namespace Shortix.Commons.Core.Exceptions
{
    public sealed class ShortixException : Exception
    {
        public ShortixException(string requestName, Error? error = default, Exception? innerException = default)
            : base("Application exception", innerException)
        {
            RequestName = requestName;
            Error = error;
        }

        public string RequestName { get; }

        public Error? Error { get; }
    }
}