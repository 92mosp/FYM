using System.Net;
using System.Text.Json;

public class ExceptionMiddleware {
    private readonly RequestDelegate _next;
    public ExceptionMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new { message = ex.Message, statusCode = context.Response.StatusCode };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
