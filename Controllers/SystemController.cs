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

        var baseUrl = _configuration["TextGenerateService:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            // Different fallbacks based on environment
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                baseUrl = "https://text-generate-services.onrender.com";
            }
            else
            {
                baseUrl = "http://127.0.0.1:8000";
            }
        }
        
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("TextGenerateService:Timeout", 120));
        
        _logger.LogInformation("HttpClient configured with BaseUrl: {BaseUrl}, Timeout: {Timeout}s", 
            _httpClient.BaseAddress, _httpClient.Timeout.TotalSeconds);
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

    [HttpGet("health/text-generate")]
    public async Task<ActionResult> TextGenerateHealthCheck(CancellationToken ct)
    {
        try
        {
            var health = await CheckTextGenerateServiceHealth();
            if (health.status == "healthy")
            {
                return Ok(health);
            }
            return StatusCode(503, health); // Service Unavailable
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking text-generate service health");
            return StatusCode(503, new { 
                status = "error", 
                error = ex.Message,
                lastChecked = DateTimeOffset.UtcNow
            });
        }
    }

    private async Task<dynamic> CheckTextGenerateServiceHealth()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var retryCount = _configuration.GetValue<int>("TextGenerateService:RetryCount", 3);
        
        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                _logger.LogInformation("Checking text-generate service health, attempt {Attempt}/{RetryCount}", attempt, retryCount);
                
                var response = await _httpClient.GetAsync("/api/system/server-info");
                stopwatch.Stop();
                var responseTimeMs = stopwatch.ElapsedMilliseconds;
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var serverInfo = JsonSerializer.Deserialize<object>(content);
                    
                    _logger.LogInformation("Text-generate service health check successful on attempt {Attempt}, response time: {ResponseTime}ms", attempt, responseTimeMs);
                    
                    return new
                    {
                        status = "healthy",
                        responseTimeMs = responseTimeMs,
                        responseTime = $"{responseTimeMs}ms",
                        lastChecked = DateTimeOffset.UtcNow,
                        attemptsUsed = attempt,
                        serverInfo = serverInfo
                    };
                }
                
                _logger.LogWarning("Text-generate service returned {StatusCode} on attempt {Attempt}", response.StatusCode, attempt);
                
                if (attempt == retryCount)
                {
                    return new
                    {
                        status = "unhealthy",
                        responseTimeMs = responseTimeMs,
                        responseTime = $"{responseTimeMs}ms",
                        statusCode = (int)response.StatusCode,
                        reason = response.ReasonPhrase,
                        lastChecked = DateTimeOffset.UtcNow,
                        attemptsUsed = attempt
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP request failed on attempt {Attempt}: {Message}", attempt, ex.Message);
                
                if (attempt == retryCount)
                {
                    stopwatch.Stop();
                    return new
                    {
                        status = "unreachable",
                        responseTimeMs = stopwatch.ElapsedMilliseconds,
                        responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                        error = ex.Message,
                        lastChecked = DateTimeOffset.UtcNow,
                        attemptsUsed = attempt
                    };
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Request timeout on attempt {Attempt}: {Message}", attempt, ex.Message);
                
                if (attempt == retryCount)
                {
                    stopwatch.Stop();
                    return new
                    {
                        status = "timeout",
                        responseTimeMs = stopwatch.ElapsedMilliseconds,
                        responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                        error = "Request timeout",
                        details = ex.Message,
                        lastChecked = DateTimeOffset.UtcNow,
                        attemptsUsed = attempt
                    };
                }
            }
            
            // Wait before retry (except on last attempt)
            if (attempt < retryCount)
            {
                var delayMs = attempt * 2000; // Progressive delay: 2s, 4s, 6s...
                _logger.LogInformation("Waiting {DelayMs}ms before retry attempt {NextAttempt}", delayMs, attempt + 1);
                await Task.Delay(delayMs);
                stopwatch.Restart(); // Restart timer for next attempt
            }
        }
        
        // This should never be reached, but just in case
        stopwatch.Stop();
        return new
        {
            status = "unknown",
            error = "Unexpected end of retry loop",
            responseTimeMs = stopwatch.ElapsedMilliseconds,
            responseTime = $"{stopwatch.ElapsedMilliseconds}ms",
            lastChecked = DateTimeOffset.UtcNow,
            attemptsUsed = retryCount
        };
    }
}
