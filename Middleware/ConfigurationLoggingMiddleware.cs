using ApiGateway.Services;

namespace ApiGateway.Middleware;

public class ConfigurationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConfigurationLoggingMiddleware> _logger;
    private static bool _hasLogged = false;

    public ConfigurationLoggingMiddleware(RequestDelegate next, ILogger<ConfigurationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ServiceEndpointsService endpointsService)
    {
        // Log configuration only once during application startup
        if (!_hasLogged)
        {
            _hasLogged = true;
            LogAllConfiguration(endpointsService);
        }

        await _next(context);
    }

    private void LogAllConfiguration(ServiceEndpointsService endpointsService)
    {
        var config = endpointsService.GetConfig();
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("    API GATEWAY CONFIGURATION SUMMARY    ");
        _logger.LogInformation("========================================");
        
        _logger.LogInformation("🌐 SERVICE ENDPOINTS:");
        _logger.LogInformation("  ├─ Text Generate URL: {Url}", config.TextGenerateBaseUrl);
        _logger.LogInformation("  ├─ Health Check Path: {Path}", config.TextGenerateHealthPath);
        _logger.LogInformation("  └─ Timeout: {Timeout}s", config.TextGenerateTimeout);
        
        _logger.LogInformation("🔄 RELIABILITY:");
        _logger.LogInformation("  ├─ Retry Count: {Count}", config.TextGenerateRetryCount);
        _logger.LogInformation("  ├─ Health Check Interval: {Interval}", config.HealthCheckInterval);
        _logger.LogInformation("  ├─ Health Check Timeout: {Timeout}", config.HealthCheckTimeout);
        _logger.LogInformation("  └─ Reactivation Period: {Period}", config.PassiveHealthReactivationPeriod);
        
        _logger.LogInformation("⚖️  LOAD BALANCING:");
        _logger.LogInformation("  └─ Policy: {Policy}", config.LoadBalancingPolicy);
        
        _logger.LogInformation("🔐 CORS SETTINGS:");
        _logger.LogInformation("  ├─ Allowed Origins: {Origins}", config.AllowedOrigins);
        _logger.LogInformation("  ├─ Allowed Methods: {Methods}", config.AllowedMethods);
        _logger.LogInformation("  └─ Allowed Headers: {Headers}", config.AllowedHeaders);
        
        _logger.LogInformation("⚙️  ENVIRONMENT:");
        _logger.LogInformation("  ├─ Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        _logger.LogInformation("  ├─ URLs: {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));
        _logger.LogInformation("  └─ Port: {Port}", Environment.GetEnvironmentVariable("PORT"));
        
        _logger.LogInformation("========================================");
    }
}
