using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests.Helpers
{
    public static class MockHelper
    {
        public static (Mock<IHttpContextAccessor> MockAccessor, DefaultHttpContext HttpContext) CreateMockHttpContext(
            Guid? userId = null,
            string email = "test@example.com",
            string role = "Admin",
            Dictionary<string, string>? cookies = null,
            Dictionary<string, string>? headers = null)
        {
            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }
            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            httpContext.User = new ClaimsPrincipal(identity);

            if (cookies != null)
            {
                var cookieHeader = string.Join("; ", cookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                httpContext.Request.Headers["Cookie"] = cookieHeader;
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpContext.Request.Headers[header.Key] = header.Value;
                }
            }

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            return (mockAccessor, httpContext);
        }
    }
}
