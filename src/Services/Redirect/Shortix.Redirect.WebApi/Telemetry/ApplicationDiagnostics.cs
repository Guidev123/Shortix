using System.Diagnostics.Metrics;

namespace Shortix.Redirect.WebApi.Telemetry
{
    public static class ApplicationDiagnostics
    {
        private const string ServiceName = "Shortix.Redirect.WebApi";
        public static readonly Meter Meter = new(ServiceName);

        public static readonly Counter<long> RedirectsCounter =
            Meter.CreateCounter<long>("redirects", description: "Number of redirects performed");
    }
}