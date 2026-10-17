using System.Text;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace SmartAndPro.ERP.WebAPI.Middlewares
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<SwaggerBasicAuthMiddleware> _logger;

        public SwaggerBasicAuthMiddleware(
            RequestDelegate next,
            IConfiguration config,
            ILogger<SwaggerBasicAuthMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            string username = _config["SwaggerAuth:Username"];
            string password = _config["SwaggerAuth:Password"];

            string authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
            {
                var encoded = authHeader.Substring("Basic ".Length).Trim();
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':');

                if (parts.Length == 2 &&
                    parts[0] == username &&
                    parts[1] == password)
                {
                    await _next(context);
                    return;
                }
            }

            _logger.LogWarning(
                "Swagger unauthorized access from IP: {IP}",
                context.Connection.RemoteIpAddress?.ToString());

            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
