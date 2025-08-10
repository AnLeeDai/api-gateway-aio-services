using ApiGateway.Middleware;
using ApiGateway.Services;
using ApiGateway.Filters;
using ApiGateway.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables and configuration
builder.Services.AddAppConfiguration(builder.Configuration);

// Get environment configuration
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? (isProduction ? "10000" : "5257"));

Console.WriteLine($"🔍 Environment: {environment}");
Console.WriteLine($"🔍 Is Production: {isProduction}");
Console.WriteLine($"🔍 Port: {port}");
Console.WriteLine($"🔍 Text Generate Base URL: {Environment.GetEnvironmentVariable("TEXT_GENERATE_BASE_URL")}");

// Configure URLs
if (isProduction)
{
    var urls = $"http://0.0.0.0:{port}";
    builder.WebHost.UseUrls(urls);
    Console.WriteLine($"🚀 Production mode: Binding to {urls}");
    
    // Add health checks for production
    builder.Services.AddHealthChecks();
}
else
{
    // Check if port is available in development
    if (!IsPortAvailable(port))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ ERROR: Port {port} is already in use!");
        Console.WriteLine($"🚫 CANNOT START API Gateway - PORT {port} IS OCCUPIED");
        Console.WriteLine($"Please stop the process using port {port} or wait for it to be available.");
        Console.ResetColor();
        Environment.Exit(1);
    }
    
    builder.WebHost.UseUrls($"http://localhost:{port}");
    Console.WriteLine($"🔧 Development mode: Binding to http://localhost:{port}");
}

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<UnifiedResponseFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient
builder.Services.AddHttpClient();

// Add custom services
builder.Services.AddScoped<SystemMetricsService>();
builder.Services.AddSingleton<ServiceEndpointsService>();

// Add YARP (Yet Another Reverse Proxy) - Temporarily disabled due to .NET 9 compatibility
// builder.Services.AddReverseProxy()
//     .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS configuration from environment variables
builder.Services.AddCors(options =>
{
    var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "*";
    var allowedMethods = Environment.GetEnvironmentVariable("CORS_ALLOWED_METHODS") ?? "GET,POST,PUT,DELETE,OPTIONS";
    var allowedHeaders = Environment.GetEnvironmentVariable("CORS_ALLOWED_HEADERS") ?? "*";
    
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins == "*")
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
            policy.WithOrigins(origins);
        }
        
        if (allowedMethods == "*")
        {
            policy.AllowAnyMethod();
        }
        else
        {
            var methods = allowedMethods.Split(',', StringSplitOptions.RemoveEmptyEntries);
            policy.WithMethods(methods);
        }
        
        if (allowedHeaders == "*")
        {
            policy.AllowAnyHeader();
        }
        else
        {
            var headers = allowedHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries);
            policy.WithHeaders(headers);
        }
    });
});

var app = builder.Build();

// Log configuration values for debugging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configuration = app.Services.GetRequiredService<IConfiguration>();

logger.LogInformation("=== Configuration Debug Info ===");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("TextGenerateService:BaseUrl = {BaseUrl}", configuration["TextGenerateService:BaseUrl"]);
logger.LogInformation("TextGenerateService:Timeout = {Timeout}", configuration["TextGenerateService:Timeout"]);
logger.LogInformation("TextGenerateService:RetryCount = {RetryCount}", configuration["TextGenerateService:RetryCount"]);
logger.LogInformation("ASPNETCORE_ENVIRONMENT = {AspNetCoreEnv}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
logger.LogInformation("=== End Configuration Debug ===");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception middleware first
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add configuration logging middleware (runs once)
app.UseMiddleware<ConfigurationLoggingMiddleware>();

// Use CORS
app.UseCors();

// Use routing
app.UseRouting();

// Map controllers
app.MapControllers();

// Add health check endpoint for production
if (isProduction)
{
    app.MapHealthChecks("/health");
    
    // Log startup information for production debugging
    Console.WriteLine($"🌐 Application will be available at: http://0.0.0.0:{port}");
    Console.WriteLine($"🏥 Health check endpoint: http://0.0.0.0:{port}/health");
    Console.WriteLine($"📊 System info endpoint: http://0.0.0.0:{port}/api/system/server-info");
}

app.Run();

// Helper method to check if port is available
static bool IsPortAvailable(int port)
{
    try
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpListeners = ipGlobalProperties.GetActiveTcpListeners();
        
        return !tcpListeners.Any(listener => listener.Port == port);
    }
    catch
    {
        return false;
    }
}
