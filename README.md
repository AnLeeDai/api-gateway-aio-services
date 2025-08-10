# API Gateway Service

API Gateway dành cho hệ thống microservices, được xây dựng với .NET 9 và YARP.

## 🚀 Chạy Local

### Yêu cầu
- .NET 9 SDK
- Backend services đang chạy (ví dụ: text-generate service trên port 8000)

### Bước 1: Clone và cài đặt
```bash
git clone <repository-url>
cd api-gateway-aio-services
dotnet restore
```

### Bước 2: Cấu hình
Tạo file `.env` hoặc set biến môi trường:
```bash
# Port gateway
PORT=5257

# Backend service URL
TEXT_GENERATE_BASE_URL=http://127.0.0.1:8000

# Timeout (giây)
TEXT_GENERATE_TIMEOUT=30
```

### Bước 3: Chạy
```bash
dotnet run
```

**Truy cập:**
- API Gateway: `http://localhost:5257`
- Swagger: `http://localhost:5257/swagger`

## 🔧 Thêm Service Mới

### Bước 1: Thêm cấu hình service
Trong `Configuration/AppConfiguration.cs`:
```csharp
public class NewServiceConfig
{
    public const string SectionName = "NewService";
    
    public string BaseUrl { get; set; } = "http://127.0.0.1:9000";
    public int Timeout { get; set; } = 30;
}
```

### Bước 2: Cập nhật DynamicProxyConfigProvider
Trong `Services/DynamicProxyConfigProvider.cs`, thêm route và cluster mới:

**Thêm route:**
```csharp
new RouteConfig
{
    RouteId = "new-service-api",
    ClusterId = "cluster-new-service",
    Match = new RouteMatch
    {
        Path = "/api/new-service/{**catchall}"
    },
    Transforms = new[]
    {
        new Dictionary<string, string> { { "PathPattern", "/api/{**catchall}" } }
    }
}
```

**Thêm cluster:**
```csharp
new ClusterConfig
{
    ClusterId = "cluster-new-service",
    LoadBalancingPolicy = "RoundRobin",
    Destinations = new Dictionary<string, DestinationConfig>
    {
        { "primary", new DestinationConfig { Address = newServiceConfig.BaseUrl } }
    }
}
```

### Bước 3: Đăng ký trong Program.cs
```csharp
builder.Services.Configure<NewServiceConfig>(
    builder.Configuration.GetSection(NewServiceConfig.SectionName));
```

### Bước 4: Thêm biến môi trường
```bash
NEW_SERVICE_BASE_URL=http://127.0.0.1:9000
NEW_SERVICE_TIMEOUT=30
```

## 📡 API Routes

| Route | Đích đến | Mô tả |
|-------|----------|-------|
| `/api/text-generate/*` | Text Generate Service | API xử lý text |
| `/api/bank-bill/*` | Text Generate Service | API bank bill |
| `/api/system/server-info` | Gateway | Thông tin hệ thống |
| `/health` | Gateway | Health check |

## 🐳 Docker

### Development
```bash
docker-compose up --build
```

### Production
```bash
docker build -t api-gateway .
docker run -p 10000:10000 -e PORT=10000 api-gateway
```

## 🔧 Cấu hình môi trường

| Biến | Mô tả | Mặc định |
|------|-------|----------|
| `PORT` | Port gateway | `5257` |
| `TEXT_GENERATE_BASE_URL` | URL backend service | `http://127.0.0.1:8000` |
| `TEXT_GENERATE_TIMEOUT` | Timeout (giây) | `30` |
| `HEALTH_CHECK_INTERVAL` | Interval health check | `00:00:30` |

## 📁 Cấu trúc thư mục

```
├── Configuration/     # Cấu hình services
├── Controllers/       # API controllers  
├── Services/         # Proxy configuration
├── Models/           # Data models
├── Middleware/       # Custom middleware
└── Program.cs        # Entry point
```
