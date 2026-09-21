using FHIR_IHE_API.Services;

namespace FHIR_IHE_API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            AuditService auditService)
        {
            bool shouldAudit = false;
            try
            {
                // ----------------------------------------
                // Continue processing the request
                // ----------------------------------------

                await _next(httpContext);

            }
            finally
            {

                // ----------------------------------------
                // Only audit API requests
                // ----------------------------------------

                if (httpContext.Request.Path.StartsWithSegments("/api"))
                {
                    shouldAudit = true;
                }

                if (shouldAudit)
                {
                    // ----------------------------------------
                    // Determine resource information
                    // ----------------------------------------

                    var resourceType = GetResourceType(httpContext);

                    var resourceId = GetResourceId(httpContext);

                    // ----------------------------------------
                    // Determine action
                    // ----------------------------------------

                    var action = httpContext.Request.Method;

                    // ----------------------------------------
                    // Write audit record
                    // ----------------------------------------

                    await auditService.LogAsync(
                        httpContext,
                        action,
                        resourceType,
                        resourceId,
                        httpContext.Response.StatusCode);
                }
            }
        }

        private static string? GetResourceType(HttpContext httpContext)
        {
            // Example:
            // /api/patient
            // /api/patient/123

            var segments = httpContext.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments == null || segments.Length < 2)
            {
                return null;
            }

            // segments[0] = api
            // segments[1] = patient
            return segments[1];
        }

        private static string? GetResourceId(HttpContext httpContext)
        {
            // first try route value "id"
            if (httpContext.Request.RouteValues.TryGetValue("id", out var routeId))
            {
                return routeId?.ToString();
            }

            // If there is no route id,
            // return null.

            return null;
        }

    }
}
