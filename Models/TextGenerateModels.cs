using System.Text.Json.Serialization;

namespace ApiGateway.Models;

public class BankBillGenerateRequest
{
    [JsonPropertyName("template")]
    public string? Template { get; set; }
    
    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }
    
    [JsonPropertyName("format")]
    public string Format { get; set; } = "pdf";
    
    [JsonPropertyName("options")]
    public BankBillOptions? Options { get; set; }
}

public class BankBillOptions
{
    [JsonPropertyName("page_size")]
    public string PageSize { get; set; } = "A4";
    
    [JsonPropertyName("orientation")]
    public string Orientation { get; set; } = "portrait";
    
    [JsonPropertyName("margin")]
    public BankBillMargin? Margin { get; set; }
}

public class BankBillMargin
{
    [JsonPropertyName("top")]
    public string Top { get; set; } = "20mm";
    
    [JsonPropertyName("right")]
    public string Right { get; set; } = "20mm";
    
    [JsonPropertyName("bottom")]
    public string Bottom { get; set; } = "20mm";
    
    [JsonPropertyName("left")]
    public string Left { get; set; } = "20mm";
}

public class TextGenerateServiceResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public class FileListResponse
{
    [JsonPropertyName("files")]
    public List<FileInfo>? Files { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class FileInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ServerInfoResponse
{
    [JsonPropertyName("server")]
    public string? Server { get; set; }
    
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    
    [JsonPropertyName("php_version")]
    public string? PhpVersion { get; set; }
    
    [JsonPropertyName("laravel_version")]
    public string? LaravelVersion { get; set; }
    
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }
    
    [JsonPropertyName("uptime")]
    public string? Uptime { get; set; }
    
    [JsonPropertyName("memory")]
    public MemoryUsage? Memory { get; set; }
    
    [JsonPropertyName("disk")]
    public DiskUsage? Disk { get; set; }
}

public class MemoryUsage
{
    [JsonPropertyName("used_gb")]
    public double UsedGb { get; set; }
    
    [JsonPropertyName("free_gb")]
    public double FreeGb { get; set; }
    
    [JsonPropertyName("total_gb")]
    public double TotalGb { get; set; }
    
    [JsonPropertyName("percent_used")]
    public double PercentUsed { get; set; }
}

public class DiskUsage
{
    [JsonPropertyName("used_gb")]
    public double UsedGb { get; set; }
    
    [JsonPropertyName("free_gb")]
    public double FreeGb { get; set; }
    
    [JsonPropertyName("total_gb")]
    public double TotalGb { get; set; }
    
    [JsonPropertyName("percent_used")]
    public double PercentUsed { get; set; }
}