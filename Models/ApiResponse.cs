namespace ApiGateway.Models;

public sealed class ApiResponse<T>
{
    public string Status { get; init; } = "ok";          // English key
    public string Message { get; init; } = "Thành công"; // Vietnamese message
    public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
    public T? Data { get; init; }
}

public sealed class ApiError
{
    public string Status { get; init; } = "error";
    public string Code { get; init; } = "internal_error";
    public string Message { get; init; } = "Đã xảy ra lỗi không mong muốn";
    public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
    public string? Detail { get; init; }
}
