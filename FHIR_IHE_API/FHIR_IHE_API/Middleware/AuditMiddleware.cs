using FHIR_IHE_API.Services;

namespace FHIR_IHE_API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(
            RequestDelegate next,
            ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AuditService auditService)
        {
            try
            {
                await _next(context);
            }
            finally
            {
                // Wrap the logic in an if statement instead of using return
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    var resourceType = context.Request.RouteValues["controller"]?.ToString();
                    var resourceId = context.Request.RouteValues["id"]?.ToString();
                    var action = context.Request.Method;

                    try
                    {
                        await auditService.LogAsync(
                            context,
                            action,
                            resourceType,
                            resourceId,
                            context.Response.StatusCode
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to create audit log for {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path
                        );
                    }
                }
            }
        }
    }
}
