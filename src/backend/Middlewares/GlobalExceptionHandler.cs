using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Backend.Middlewares
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message) : base(message) { }
    }


    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }


        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, exception.Message),
                InternalServerErrorException => (StatusCodes.Status500InternalServerError, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled error on HTTP {Method} {Path}: {Message}", httpContext.Request.Method, httpContext.Request.Path, message);
            }
            else
            {
                _logger.LogWarning("Handled domain exception ({StatusCode}) on HTTP {Method} {Path}: {Message}", statusCode, httpContext.Request.Method, httpContext.Request.Path, message);
            }

            var responseObj = new
            {
                status = statusCode,
                error = message
            };

            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                responseObj,
                responseObj.GetType(),
                cancellationToken: cancellationToken
            );

            return true;
        }
    }
}
