using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("")]
public class SystemController : ControllerBase
{
    private readonly SystemMetricsService _metrics;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemController> _logger;
    
    public SystemController(
        SystemMetricsService metrics,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SystemController> logger)
    {
        _metrics = metrics;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["TextGenerateService:BaseUrl"] ?? "http://text-generate-app:80";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("TextGenerateService:Timeout", 30));
    }

    // Root returns raw data; UnifiedResponseFilter will wrap it to ApiResponse
    [HttpGet("")]
    public async Task<ActionResult<SystemSummaryDto>> Root(CancellationToken ct)
    {
        var data = await _metrics.GetSystemSummaryAsync(ct);
        
        // Add text-generate service status with timing
        try
        {
            var textGenerateHealth = await CheckTextGenerateServiceHealth();
            data.Services = new Dictionary<string, object>
            {
                ["text-generate"] = textGenerateHealth
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get text-generate service health");
            data.Services = new Dictionary<string, object>
            {
                ["text-generate"] = new { 
                    status = "unknown", 
                    error = ex.Message,
                    responseTimeMs = 0,
                    responseTime = "0ms",
                    lastChecked = DateTimeOffset.UtcNow
                }
            };
        }
        
        return Ok(data);
    }

    // Health endpoint unified: return raw and let filter wrap
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var gatewayHealth = new { value = "healthy", time = DateTimeOffset.UtcNow };
        
        try
        {
            var textGenerateHealth = await CheckTextGenerateServiceHealth();
            var combinedHealth = new
            {
                gateway = gatewayHealth,
                services = new
                {
                    textGenerate = textGenerateHealth
                },
                overall = textGenerateHealth.status == "healthy" ? "healthy" : "degraded"
            };
            
            return Ok(combinedHealth);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check text-generate service health");
            var degradedHealth = new
            {
                gateway = gatewayHealth,
                services = new
                {
                    textGenerate = new { 
                        status = "unknown", 
                        error = ex.Message,
                        responseTimeMs = 0,
                        responseTime = "0ms",
                        lastChecked = DateTimeOffset.UtcNow
                    }
                },
                overall = "degraded"
            };
            
            return Ok(degradedHealth);
        }
    }

    private async Task<dynamic> CheckTextGenerateServiceHealth()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var response = await _httpClient.GetAsync("/api/system/server-info");
            stopwatch.Stop();
            var responseTimeMs = stopwatch.ElapsedMilliseconds;
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var serverInfo = JsonSerializer.Deserialize<object>(content);
                
                return new
                {
                    status = "healthy",
                    responseTimeMs = responseTimeMs,
                    responseTime = $"{responseTimeMs}ms",
                    lastChecked = DateTimeOffset.UtcNow,
                    serverInfo = serverInfo
                };
            }
            
            return new
            {
                status = "unhealthy",
                responseTimeMs = responseTimeMs,
                responseTime = $"{responseTimeMs}ms",
                statusCode = (int)response.StatusCode,
                reason = response.ReasonPhrase,
                lastChecked = DateTimeOffset.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return new
            {
                status = "unreachable",
                responseTimeMs = stopwatch.ElapsedMilliseconds,
                responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                error = ex.Message,
                lastChecked = DateTimeOffset.UtcNow
            };
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            return new
            {
                status = "timeout",
                responseTimeMs = stopwatch.ElapsedMilliseconds,
                responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                error = "Request timeout",
                details = ex.Message,
                lastChecked = DateTimeOffset.UtcNow
            };
        }
    }
}
