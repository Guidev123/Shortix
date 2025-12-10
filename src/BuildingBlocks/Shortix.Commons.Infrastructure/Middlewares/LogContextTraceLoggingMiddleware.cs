using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Shortix.Commons.Infrastructure.Middlewares
{
    public sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
    {
        public Task Invoke(HttpContext context)
        {
            var traceId = Activity.Current?.TraceId.ToString();

            using (LogContext.PushProperty("TraceId", traceId))
            {
                return next.Invoke(context);
            }
        }
    }
}