using System.Net;
using System.Text.Json;
using ApiGateway.Models;

namespace ApiGateway.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var payload = new ApiError
            {
                Status = "error",
                Code = "internal_error",
                Message = "Đã xảy ra lỗi không mong muốn",
                Detail = ex.Message,
                Time = DateTimeOffset.UtcNow
            };
            var json = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(json);
        }
    }
}
