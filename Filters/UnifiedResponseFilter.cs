using ApiGateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGateway.Filters;

public class UnifiedResponseFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Only wrap 2xx responses that are ObjectResult and not already wrapped
        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? 200;
            if (statusCode >= 200 && statusCode < 300)
            {
                var value = objectResult.Value;
                // Avoid double-wrapping if value already is ApiResponse<>
                var valueType = value?.GetType();
                var isAlreadyWrapped = valueType != null &&
                    valueType.IsGenericType &&
                    valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>);
                if (!isAlreadyWrapped)
                {
                    var wrapped = new ApiResponse<object?>
                    {
                        Status = "ok",
                        Message = "Thành công",
                        Time = DateTimeOffset.UtcNow,
                        Data = value
                    };
                    context.Result = new ObjectResult(wrapped)
                    {
                        StatusCode = statusCode
                    };
                }
            }
        }
        return next();
    }
}
