using Backend.Helpers;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;

    public TokenBlacklistMiddleware(RequestDelegate next, ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }


    /// <summary>
    /// This middleware checks if the incoming request has a JWT token that is blacklisted. If the token is blacklisted, it returns a 401 Unauthorized response. Otherwise, it passes the request to the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The HttpContext for the current request.</param>
    /// <param name="blacklistRepo">The repository used to check if a token is blacklisted.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ITokenBlacklistRepository blacklistRepo)
    {
        // Check if the Authorization header is present and starts with "Bearer "
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        // If the Authorization header is present and starts with "Bearer ", extract the token and check if it is blacklisted
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // The middleware no longer cares if this is checking Redis, SQL, or Memory
            if (await blacklistRepo.IsBlacklistedAsync(token))
            {
                _logger.LogWarning("TokenBlacklistMiddleware: Revoked token attempt on {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Token revoked" });
                return;
            }
        }

        await _next(context);
    }
}