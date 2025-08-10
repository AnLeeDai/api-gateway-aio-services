using ApiGateway.Middleware;
using ApiGateway.Services;
using ApiGateway.Filters;
using System.Net;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs based on environment
var environment = builder.Environment.EnvironmentName;
var isProduction = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

if (isProduction)
{
    // For production (Render), use the PORT environment variable or default to 10000
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? $"http://0.0.0.0:{port}";
    builder.WebHost.UseUrls(urls);
    Console.WriteLine($"🚀 Production mode: Binding to {urls}");
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

// Add YARP (Yet Another Reverse Proxy) - Temporarily disabled due to .NET 9 compatibility
// builder.Services.AddReverseProxy()
//     .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception middleware first
app.UseMiddleware<GlobalExceptionMiddleware>();

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
