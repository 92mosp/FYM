namespace FYMTechnology.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        context.Response.Headers.ReferrerPolicy = "no-referrer";
        context.Response.Headers.PermissionsPolicy = "camera=(), microphone=(), geolocation=()";
        await next(context);
    }
}
