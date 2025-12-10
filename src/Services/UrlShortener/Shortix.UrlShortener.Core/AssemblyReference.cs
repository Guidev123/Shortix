using System.Reflection;

namespace Shortix.UrlShortener.Core
{
    public static class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}