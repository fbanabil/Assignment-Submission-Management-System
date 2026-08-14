using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var path = request.Path;
            var method = request.Method;
            var traceId = context.TraceIdentifier;

            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            using (_logger.BeginScope("TraceId:{TraceId} User:{UserId}", traceId, userId))
            {
                _logger.LogInformation("HTTP {Method} {Path} started", method, path);

                try
                {
                    await _next(context);
                    stopwatch.Stop();

                    _logger.LogInformation("HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms",
                        method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "HTTP {Method} {Path} failed with exception after {ElapsedMs}ms",
                        method, path, stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }
    }
}
