# API Gateway với Text Generate Services Integration

Hệ thống API Gateway được thiết lập để gọi đến text-generate-services thông qua hai cách:
1. **Controller-based routing**: Xử lý logic trong controller và forward request
2. **YARP Reverse Proxy**: Trực tiếp proxy request đến text-generate-services

## Kiến trúc

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client        │    │  API Gateway    │    │ Text Generate   │
│                 │    │                 │    │ Services        │
│                 │───▶│                 │───▶│                 │
│                 │    │ - Controllers   │    │ - Laravel App   │
│                 │    │ - YARP Proxy    │    │ - PHP/Apache    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Cài đặt và Khởi chạy

### 1. Chuẩn bị

Đảm bảo bạn có cấu trúc thư mục như sau:
```
services/
├── api-gateway-aio-services/
│   ├── net9.0/                 # Built API Gateway
│   ├── scripts/
│   └── docker-compose.yml
└── text-generate-services/     # Laravel application
    ├── app/
    ├── public/
    └── Dockerfile
```

### 2. Khởi chạy hệ thống

```bash
cd api-gateway-aio-services
./scripts/start-services.sh
```

Script này sẽ:
- Dừng các container cũ (nếu có)
- Build và khởi chạy các services
- Kiểm tra health của các services
- Hiển thị các endpoints có sẵn

### 3. Kiểm tra API

```bash
./scripts/test-api.sh
```

## Endpoints

### Gateway Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Gateway system information |
| GET | `/health` | Gateway và services health check |

### Text Generate Service (via Controller)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/text-generate/health` | Service health check |
| GET | `/api/text-generate/system/server-info` | Server information |
| GET | `/api/text-generate/system/list` | List generated files |
| DELETE | `/api/text-generate/system/delete-all` | Delete all files |
| DELETE | `/api/text-generate/system/delete/{fileName}` | Delete specific file |
| POST | `/api/text-generate/bank-bill/generate` | Generate bank bill |

### Text Generate Service (via YARP Proxy)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/system/server-info` | Server information |
| GET | `/api/system/list` | List generated files |
| DELETE | `/api/system/delete-all` | Delete all files |
| DELETE | `/api/system/delete/{fileName}` | Delete specific file |
| POST | `/api/bank-bill/generate` | Generate bank bill |

## Ví dụ sử dụng

### 1. Kiểm tra health

```bash
curl http://localhost:8080/health
```

### 2. Lấy thông tin server

```bash
# Via Controller
curl http://localhost:8080/api/text-generate/system/server-info

# Via YARP Proxy
curl http://localhost:8080/api/system/server-info
```

### 3. Generate Bank Bill

```bash
curl -X POST http://localhost:8080/api/bank-bill/generate \
  -H "Content-Type: application/json" \
  -d '{
    "template": "bank_statement",
    "data": {
      "account_number": "123456789",
      "account_holder": "John Doe",
      "bank_name": "Test Bank",
      "statement_period": "January 2024",
      "transactions": [
        {
          "date": "2024-01-01",
          "description": "Initial Deposit",
          "amount": 1000.00,
          "balance": 1000.00
        }
      ]
    },
    "format": "pdf"
  }'
```

## Cấu hình

### API Gateway Configuration (appsettings.json)

```json
{
  "ReverseProxy": {
    "Routes": {
      "text-generate-api": {
        "ClusterId": "cluster-text-generate",
        "Match": { "Path": "/api/text-generate/{**catchall}" }
      }
    },
    "Clusters": {
      "cluster-text-generate": {
        "Destinations": {
          "primary": { "Address": "http://text-generate-app:80/" }
        }
      }
    }
  },
  "TextGenerateService": {
    "BaseUrl": "http://text-generate-app:80",
    "Timeout": 30,
    "RetryCount": 3
  }
}
```

### Docker Compose

Cả hai services chạy trong cùng network `app-network`:
- **api-gateway**: Port 8080 (exposed)
- **text-generate-app**: Internal communication only

## Troubleshooting

### 1. Services không start

```bash
docker-compose logs api-gateway
docker-compose logs text-generate-app
```

### 2. Health check thất bại

Kiểm tra network connectivity:
```bash
docker exec api-gateway curl -f http://text-generate-app:80/api/system/server-info
```

### 3. Text Generate Service không response

Kiểm tra Laravel application:
```bash
docker exec text-generate-app php artisan --version
docker exec text-generate-app curl -f http://localhost/api/system/server-info
```

## Development

### Rebuild services

```bash
docker-compose down
docker-compose up --build
```

### View logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api-gateway
docker-compose logs -f text-generate-app
```

### Stop services

```bash
docker-compose down
```

## Features

### 1. Dual Routing Strategy

- **Controller-based**: Cho phép thêm logic, validation, transformation
- **YARP Proxy**: Direct proxy cho performance tốt hơn

### 2. Health Monitoring

- Automatic health checks
- Service dependency tracking
- Integrated health endpoints

### 3. Error Handling

- Unified error response format
- Proper HTTP status codes
- Detailed error logging

### 4. Configuration Management

- Environment-based configuration
- Service discovery via Docker networking
- Configurable timeouts và retry policies

## API Response Format

Tất cả responses đều follow unified format:

```json
{
  "success": true,
  "data": { ... },
  "message": "Success",
  "timestamp": "2024-08-10T12:00:00Z"
}
```

## Security Considerations

1. Services communication chỉ trong internal network
2. Chỉ API Gateway được expose ra ngoài
3. Health check endpoints có thể bị disable trong production
4. Consider thêm authentication/authorization cho production
