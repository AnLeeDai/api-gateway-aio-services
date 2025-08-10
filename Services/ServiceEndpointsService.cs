using Microsoft.Extensions.Options;

namespace ApiGateway.Services;

public class ServiceEndpointsConfig
{
    public string TextGenerateBaseUrl { get; set; } = "http://127.0.0.1:8000";
    public int TextGenerateTimeout { get; set; } = 120;
    public int TextGenerateRetryCount { get; set; } = 3;
    public string TextGenerateHealthPath { get; set; } = "/api/system/server-info";
    
    // Health Check Configuration
    public string HealthCheckInterval { get; set; } = "00:01:00";
    public string HealthCheckTimeout { get; set; } = "00:00:30";
    public string PassiveHealthReactivationPeriod { get; set; } = "00:00:10";
    
    // Load Balancing
    public string LoadBalancingPolicy { get; set; } = "RoundRobin";
    
    // CORS Configuration
    public string AllowedOrigins { get; set; } = "*";
    public string AllowedMethods { get; set; } = "GET,POST,PUT,DELETE,OPTIONS";
    public string AllowedHeaders { get; set; } = "*";
}

public class ServiceEndpointsService
{
    private readonly ServiceEndpointsConfig _config;
    private readonly ILogger<ServiceEndpointsService> _logger;

    public ServiceEndpointsService(ILogger<ServiceEndpointsService> logger)
    {
        _logger = logger;
        _config = LoadFromEnvironment();
        LogConfiguration();
    }

    public ServiceEndpointsConfig GetConfig() => _config;

    private ServiceEndpointsConfig LoadFromEnvironment()
    {
        var config = new ServiceEndpointsConfig();

        // Load from environment variables with fallback to defaults
        config.TextGenerateBaseUrl = Environment.GetEnvironmentVariable("TEXT_GENERATE_SERVICE_URL") 
            ?? Environment.GetEnvironmentVariable("TEXT_GENERATE_BASE_URL") 
            ?? config.TextGenerateBaseUrl;

        if (int.TryParse(Environment.GetEnvironmentVariable("TEXT_GENERATE_TIMEOUT"), out var timeout))
            config.TextGenerateTimeout = timeout;

        if (int.TryParse(Environment.GetEnvironmentVariable("TEXT_GENERATE_RETRY_COUNT"), out var retryCount))
            config.TextGenerateRetryCount = retryCount;

        config.TextGenerateHealthPath = Environment.GetEnvironmentVariable("TEXT_GENERATE_HEALTH_PATH") 
            ?? config.TextGenerateHealthPath;

        // Health Check Configuration
        config.HealthCheckInterval = Environment.GetEnvironmentVariable("HEALTH_CHECK_INTERVAL") 
            ?? config.HealthCheckInterval;
        
        config.HealthCheckTimeout = Environment.GetEnvironmentVariable("HEALTH_CHECK_TIMEOUT") 
            ?? config.HealthCheckTimeout;
        
        config.PassiveHealthReactivationPeriod = Environment.GetEnvironmentVariable("PASSIVE_HEALTH_REACTIVATION_PERIOD") 
            ?? config.PassiveHealthReactivationPeriod;

        // Load Balancing
        config.LoadBalancingPolicy = Environment.GetEnvironmentVariable("LOAD_BALANCING_POLICY") 
            ?? config.LoadBalancingPolicy;

        // CORS Configuration
        config.AllowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") 
            ?? config.AllowedOrigins;
        
        config.AllowedMethods = Environment.GetEnvironmentVariable("CORS_ALLOWED_METHODS") 
            ?? config.AllowedMethods;
        
        config.AllowedHeaders = Environment.GetEnvironmentVariable("CORS_ALLOWED_HEADERS") 
            ?? config.AllowedHeaders;

        return config;
    }

    private void LogConfiguration()
    {
        _logger.LogInformation("=== Service Endpoints Configuration ===");
        _logger.LogInformation("TextGenerate BaseUrl: {BaseUrl}", _config.TextGenerateBaseUrl);
        _logger.LogInformation("TextGenerate Timeout: {Timeout}s", _config.TextGenerateTimeout);
        _logger.LogInformation("TextGenerate RetryCount: {RetryCount}", _config.TextGenerateRetryCount);
        _logger.LogInformation("TextGenerate HealthPath: {HealthPath}", _config.TextGenerateHealthPath);
        _logger.LogInformation("Health Check Interval: {Interval}", _config.HealthCheckInterval);
        _logger.LogInformation("Health Check Timeout: {Timeout}", _config.HealthCheckTimeout);
        _logger.LogInformation("Load Balancing Policy: {Policy}", _config.LoadBalancingPolicy);
        _logger.LogInformation("CORS Allowed Origins: {Origins}", _config.AllowedOrigins);
        _logger.LogInformation("=== End Configuration ===");
    }
}
