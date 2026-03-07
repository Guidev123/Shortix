using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shortix.CosmosDbTriggerFunction
{
    public sealed class HealthCheck(HealthCheckService healthCheckService)
    {
        [Function(nameof(HealthCheck))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,
            "get", "post",
            Route = "healthz")] HttpRequestData req,
            FunctionContext context)
        {
            var healthStatus = await healthCheckService.CheckHealthAsync();
            return new OkObjectResult(Enum.GetName(typeof(HealthStatus), healthStatus.Status));
        }
    }
}