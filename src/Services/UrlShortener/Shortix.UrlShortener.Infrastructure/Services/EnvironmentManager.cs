using Shortix.UrlShortener.Core.Interfaces;

namespace Shortix.UrlShortener.Infrastructure.Services
{
    internal sealed class EnvironmentManager : IEnvironmentManager
    {
        public void FatalError()
        {
            Environment.Exit(-1);
        }
    }
}