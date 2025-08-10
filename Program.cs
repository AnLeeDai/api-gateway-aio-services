using ApiGateway.Middleware;
using ApiGateway.Services;
using ApiGateway.Filters;
using System.Net;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs based on environment
var environment = builder.Environment.EnvironmentName;
var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

Console.WriteLine($"🔍 Environment: {environment}");
Console.WriteLine($"🔍 Is Production: {isProduction}");
Console.WriteLine($"🔍 PORT env var: {Environment.GetEnvironmentVariable("PORT")}");
Console.WriteLine($"🔍 ASPNETCORE_URLS env var: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

if (isProduction)
{
    // For production (Render), Render automatically sets the PORT environment variable
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    var urls = $"http://0.0.0.0:{port}";
    builder.WebHost.UseUrls(urls);
    Console.WriteLine($"🚀 Production mode: Binding to {urls}");
    Console.WriteLine($"🔍 Environment PORT: {Environment.GetEnvironmentVariable("PORT")}");
    Console.WriteLine($"🔍 Using port: {port}");
}
else
{
    // For development, use fixed port configuration
    const int FIXED_PORT = 5257;
    
    // Check if port is available before starting
    if (!IsPortAvailable(FIXED_PORT))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ ERROR: Port {FIXED_PORT} is already in use!");
        Console.WriteLine($"🚫 CANNOT START API Gateway - PORT {FIXED_PORT} IS OCCUPIED");
        Console.WriteLine($"Please stop the process using port {FIXED_PORT} or wait for it to be available.");
        Console.WriteLine($"Do not change to another port - this service must run on port {FIXED_PORT}.");
        Console.ResetColor();
        Environment.Exit(1);
    }
    
    builder.WebHost.UseUrls($"http://localhost:{FIXED_PORT}");
    Console.WriteLine($"🔧 Development mode: Binding to http://localhost:{FIXED_PORT}");
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

// Map YARP reverse proxy - Temporarily disabled due to .NET 9 compatibility
// app.MapReverseProxy();

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
