using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace certifyab.Middleware
{
    public class ErrorMetricMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorMetricMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, TelemetryClient telemetryClient)
        {
            await _next(context);

            if (context.Response.StatusCode >= 400)
                telemetryClient.GetMetric("FailedRequests").TrackValue(1);
        }
    }
}