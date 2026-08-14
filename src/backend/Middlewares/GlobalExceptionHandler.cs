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
