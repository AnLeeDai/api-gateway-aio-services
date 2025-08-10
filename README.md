# API Gateway (.NET 9 + YARP)

## Quick Start

### 1. Environment Setup
```bash
# Copy environment template
cp .env.example .env

# Edit .env with your configuration
nano .env
```

**📖 For detailed environment configuration, see [ENVIRONMENT.md](ENVIRONMENT.md)**

### 2. Production Run

```bash
# Cách 1: Dùng Makefile
make run              # Chạy trên localhost:8080
PORT=8081 make run   # Chạy trên cổng khác

# Cách 2: Chạy trực tiếp
dotnet net9.0/ApiGateway.dll
```

### 2. Chạy với Hot Reload (Development)

```bash
make dev
# hoặc
scripts/run.sh dev
```

### 3. Chạy bằng Docker

```bash
# Development
docker compose up -d

# Production
docker-compose -f docker-compose.prod.yml up -d
```

### 4. Deploy lên Render

Xem chi tiết trong [DEPLOYMENT.md](DEPLOYMENT.md)

```bash
# Test production build locally
./scripts/build-prod.sh
```

## Cách gọi đến các services khác

### 1. Kiểm tra API Gateway đã chạy

```bash
curl http://localhost:8080/api/system/server-info
```

### 2. Gọi Text Generate Service (qua Gateway)

```bash
# Thông tin server
curl http://localhost:8080/api/text-generate/system/server-info

# Generate text
curl -X POST http://localhost:8080/api/text-generate/bank-bill \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Generate bank bill"}'
```

### 3. Production Endpoints (Render)

```bash
# Health check
curl https://your-app.onrender.com/health

# System info
curl https://your-app.onrender.com/api/system/summary
```

### 3. Thêm service mới

Chỉnh file `net9.0/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "your-service": {
        "ClusterId": "cluster-your-service",
        "Match": { "Path": "/api/your-service/{**catchall}" }
      }
    },
    "Clusters": {
      "cluster-your-service": {
        "Destinations": {
          "primary": { "Address": "http://localhost:YOUR_PORT/" }
        }
      }
    }
  }
}
```

## Lưu ý

- API Gateway mặc định chạy trên `localhost:8080`
- Tất cả requests đến `/api/*` sẽ được route đến services tương ứng
- Nếu cổng 8080 bận, script tự động tìm cổng trống khác

Hoặc kiểm tra process đang dùng cổng:

```bash
ss -tlnp | grep :8080           # Kiểm tra cổng 8080
sudo fuser -n tcp 8080          # Tìm PID đang dùng cổng 8080
docker ps                       # Kiểm tra Docker containers
```

## Lệnh nhanh (cheatsheet)

- Production: `make run` hoặc `ASPNETCORE_URLS=... dotnet net9.0/ApiGateway.dll`
- Dev hot reload: `make dev` hoặc `scripts/run.sh dev` (có csproj thì hot reload, không thì fallback)
- Chỉ định cổng: `PORT=5000 scripts/run.sh dev`
