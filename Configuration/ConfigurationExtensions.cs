using ApiGateway.Configuration;

namespace ApiGateway.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Load environment variables from .env file
        LoadEnvironmentVariables();
        
        // Configure strongly typed configuration objects
        services.Configure<TextGenerateServiceConfig>(options =>
        {
            options.BaseUrl = Environment.GetEnvironmentVariable("TEXT_GENERATE_BASE_URL") ?? "http://127.0.0.1:8000";
            options.Timeout = int.Parse(Environment.GetEnvironmentVariable("TEXT_GENERATE_TIMEOUT") ?? "30");
            options.RetryCount = int.Parse(Environment.GetEnvironmentVariable("TEXT_GENERATE_RETRY_COUNT") ?? "3");
        });
        
        services.Configure<AppHealthCheckConfig>(options =>
        {
            options.Interval = Environment.GetEnvironmentVariable("HEALTH_CHECK_INTERVAL") ?? "00:00:30";
            options.Timeout = Environment.GetEnvironmentVariable("HEALTH_CHECK_TIMEOUT") ?? "00:00:05";
            options.Path = Environment.GetEnvironmentVariable("HEALTH_CHECK_PATH") ?? "/api/system/server-info";
            options.ReactivationPeriod = Environment.GetEnvironmentVariable("PASSIVE_HEALTH_REACTIVATION_PERIOD") ?? "00:00:10";
        });
        
        services.Configure<LoadBalancingConfig>(options =>
        {
            options.Policy = Environment.GetEnvironmentVariable("LOAD_BALANCING_POLICY") ?? "RoundRobin";
        });
        
        return services;
    }
    
    private static void LoadEnvironmentVariables()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var rootPath = Directory.GetCurrentDirectory();
        
        // Load environment-specific .env file first
        var envFiles = new[]
        {
            Path.Combine(rootPath, $".env.{environment.ToLower()}"),
            Path.Combine(rootPath, ".env")
        };
        
        foreach (var envFile in envFiles.Where(File.Exists))
        {
            Console.WriteLine($"🔍 Loading environment variables from: {envFile}");
            LoadEnvFile(envFile);
            break; // Load only the first matching file
        }
    }
    
    private static void LoadEnvFile(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;
                
                var equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex <= 0) continue;
                
                var key = trimmedLine.Substring(0, equalIndex).Trim();
                var value = trimmedLine.Substring(equalIndex + 1).Trim();
                
                // Remove quotes if present
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }
                
                // Only set if not already set (environment variables take precedence)
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Could not load .env file {filePath}: {ex.Message}");
        }
    }
}
