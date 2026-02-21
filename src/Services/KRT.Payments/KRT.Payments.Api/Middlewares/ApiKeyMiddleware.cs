namespace KRT.Payments.Api.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    private static readonly string[] ProtectedPrefixes =
    {
        "/api/v1/pix/charges",
        "/api/v1/boletos/charges",
        "/api/v1/cards/charges"
    };

    private static readonly string[] HealthPrefixes =
    {
        "/health",
        "/api/v1/health"
    };

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Health check endpoints always pass through
        if (HealthPrefixes.Any(h => path.StartsWith(h, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Admin endpoints — require admin API key
        if (path.StartsWith("/api/v1/admin", StringComparison.OrdinalIgnoreCase))
        {
            var adminApiKey = _configuration["Security:AdminApiKey"];
            if (string.IsNullOrEmpty(adminApiKey))
            {
                await _next(context);
                return;
            }

            var providedAdminKey = context.Request.Headers["X-Admin-Key"].FirstOrDefault()
                                ?? context.Request.Headers["X-Api-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(providedAdminKey) || providedAdminKey != adminApiKey)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Admin API key invalida ou ausente\"}");
                return;
            }

            await _next(context);
            return;
        }

        // Charges endpoints — require standard API key
        if (ProtectedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            var apiKey = _configuration["Security:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                await _next(context);
                return;
            }

            var providedKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(providedKey) || providedKey != apiKey)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"API key invalida ou ausente\"}");
                return;
            }

            await _next(context);
            return;
        }

        // All other routes pass through (protected by Keycloak [Authorize])
        await _next(context);
    }
}
