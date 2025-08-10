namespace ApiGateway.Configuration;

public class AppConfig
{
    public string Environment { get; set; } = "Development";
    public int Port { get; set; } = 5257;
    public string AllowedHosts { get; set; } = "*";
    public string LogLevel { get; set; } = "Information";
}

public class TextGenerateServiceConfig
{
    public const string SectionName = "TextGenerateService";
    
    public string BaseUrl { get; set; } = "http://127.0.0.1:8000";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}

public class AppHealthCheckConfig
{
    public const string SectionName = "HealthCheck";
    
    public string Interval { get; set; } = "00:00:30";
    public string Timeout { get; set; } = "00:00:05";
    public string Path { get; set; } = "/api/system/server-info";
    public string ReactivationPeriod { get; set; } = "00:00:10";
}

public class LoadBalancingConfig
{
    public const string SectionName = "LoadBalancing";
    
    public string Policy { get; set; } = "RoundRobin";
}
