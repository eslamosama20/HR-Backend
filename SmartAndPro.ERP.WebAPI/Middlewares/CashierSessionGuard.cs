using Microsoft.AspNetCore.Http;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Middlewares
{
    /// <summary>
    /// Middleware to enforce active cashier session for protected endpoints
    /// Ensures all POS operations require an active session with a linked store
    /// </summary>
    public class CashierSessionGuard
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CashierSessionGuard> _logger;

        // Endpoints that require active cashier session
        private static readonly string[] ProtectedPaths = new[]
        {
            "/api/cashier/pos",
            "/api/cashier/invoice",
            "/api/cashier/payment",
            "/api/cashier/checkout",
            "/api/cashier/quick-sale",
            "/api/restaurant"
        };

        // Endpoints that are excluded from guard
        private static readonly string[] ExcludedPaths = new[]
        {
            "/api/cashier/session/open",
            "/api/cashier/session/current",
            "/api/cashier/session/history"
        };

        public CashierSessionGuard(RequestDelegate next, ILogger<CashierSessionGuard> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICashierRuntimeStore cashierStore)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Check if path requires session guard
            if (RequiresSessionGuard(path))
            {
                // Get current user
                var userGuid = ResolveCurrentUserGuid(context);
                if (userGuid == Guid.Empty)
                {
                    await RespondUnauthorized(context, "User not authenticated");
                    return;
                }

                // Check for active session
                var session = cashierStore.GetCurrentSession(userGuid);
                if (session == null)
                {
                    await RespondForbidden(context, 
                        "NO_ACTIVE_SESSION",
                        "No active cashier session. Please open a session first.",
                        "لا توجد جلسة كاشير نشطة. الرجاء فتح جلسة أولاً.");
                    return;
                }

                // Validate session has store
                if (!session.StoreDetailsId.HasValue || session.StoreDetailsId.Value == Guid.Empty)
                {
                    await RespondForbidden(context,
                        "SESSION_NO_STORE",
                        "Session has no linked store. Please close and reopen with a valid store.",
                        "الجلسة غير مرتبطة بمخزن. الرجاء إغلاق الجلسة وفتحها مع مخزن صحيح.");
                    return;
                }

                _logger.LogDebug(
                    "Session guard passed for user {UserGuid}, session {SessionNumber}, store {StoreId}, path {Path}",
                    userGuid, session.SessionNumber, session.StoreDetailsId.Value, path
                );

                // Add session info to context for downstream use
                context.Items["CashierSession"] = session;
                context.Items["CashierSessionId"] = session.Id;
                context.Items["CashierStoreId"] = session.StoreDetailsId.Value;
            }

            await _next(context);
        }

        private bool RequiresSessionGuard(string path)
        {
            // First check if explicitly excluded
            if (ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Then check if protected
            return ProtectedPaths.Any(protectedPath => path.StartsWith(protectedPath, StringComparison.OrdinalIgnoreCase));
        }

        private Guid ResolveCurrentUserGuid(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User?.FindFirst(ClaimTypes.Name)?.Value  // Added for TokenService compatibility
                           ?? context.User?.FindFirst("sub")?.Value
                           ?? context.User?.FindFirst("userId")?.Value;

            if (Guid.TryParse(userIdClaim, out var userGuid))
            {
                return userGuid;
            }

            return Guid.Empty;
        }

        private async Task RespondUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 401,
                message = message
            });
        }

        private async Task RespondForbidden(HttpContext context, string errorCode, string messageEn, string messageAr)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 403,
                errorCode = errorCode,
                message = messageEn,
                messageAr = messageAr,
                timestamp = DateTime.UtcNow
            });

            _logger.LogWarning(
                "Session guard blocked request: {ErrorCode} - {Path} - User: {UserId}",
                errorCode, context.Request.Path, context.User?.Identity?.Name ?? "Unknown"
            );
        }
    }

    /// <summary>
    /// Extension methods for registering CashierSessionGuard middleware
    /// </summary>
    public static class CashierSessionGuardExtensions
    {
        public static IApplicationBuilder UseCashierSessionGuard(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CashierSessionGuard>();
        }
    }
}
