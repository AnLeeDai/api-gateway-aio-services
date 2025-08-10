# Environment Variables Configuration

## Quick Setup

### Development Setup

1. Copy the example environment file:
```bash
cp .env.example .env
```

2. Edit `.env` file with your local configuration:
```bash
# Minimal required configuration for development
TEXT_GENERATE_SERVICE_URL=http://127.0.0.1:8000
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5257
```

### Production Deployment (Render)

Set these environment variables in your Render dashboard:

## 🔧 All Available Environment Variables

### 🌐 Service Endpoints (Required)
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `TEXT_GENERATE_SERVICE_URL` | Primary text service URL | `http://127.0.0.1:8000` | `https://text-generate-services.onrender.com` |
| `TEXT_GENERATE_BASE_URL` | Alternative name for URL | Same as above | Same as above |

### ⚙️ Service Configuration
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `TEXT_GENERATE_TIMEOUT` | Request timeout (seconds) | `120` | `180` |
| `TEXT_GENERATE_RETRY_COUNT` | Number of retry attempts | `3` | `5` |
| `TEXT_GENERATE_HEALTH_PATH` | Health check endpoint | `/api/system/server-info` | `/health` |

### 🔄 Health Check Configuration
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `HEALTH_CHECK_INTERVAL` | Check frequency | `00:01:00` | `00:02:00` |
| `HEALTH_CHECK_TIMEOUT` | Check timeout | `00:00:30` | `00:01:00` |
| `PASSIVE_HEALTH_REACTIVATION_PERIOD` | Retry period | `00:00:10` | `00:00:30` |

### ⚖️ Load Balancing
| Variable | Description | Default | Options |
|----------|-------------|---------|---------|
| `LOAD_BALANCING_POLICY` | Load balancing strategy | `RoundRobin` | `RoundRobin`, `LeastRequests`, `PowerOfTwoChoices` |

### 🔐 CORS Configuration
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `CORS_ALLOWED_ORIGINS` | Allowed origins | `*` | `https://mydomain.com,https://anotherdomain.com` |
| `CORS_ALLOWED_METHODS` | Allowed HTTP methods | `GET,POST,PUT,DELETE,OPTIONS` | `GET,POST` |
| `CORS_ALLOWED_HEADERS` | Allowed headers | `*` | `Content-Type,Authorization` |

### 🖥️ ASP.NET Core Configuration
| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development` | `Production` |
| `ASPNETCORE_URLS` | Server binding URLs | `http://localhost:5257` | `http://0.0.0.0:10000` |
| `PORT` | Port for platforms | `5257` | `10000` |

### 📝 Logging
| Variable | Description | Default | Options |
|----------|-------------|---------|---------|
| `LOG_LEVEL` | Minimum log level | `Information` | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical` |

## 🏗️ Configuration Priority

The application loads configuration in this order (highest to lowest priority):

1. **Environment Variables** (highest priority) ⭐
2. **Configuration Files** (`appsettings.{Environment}.json`)
3. **Base Configuration** (`appsettings.json`)
4. **Default Values** (lowest priority)

## 🚀 Production Templates

### Render.com Configuration
```yaml
envVars:
  - key: TEXT_GENERATE_SERVICE_URL
    value: https://your-text-service.onrender.com
  - key: ASPNETCORE_ENVIRONMENT
    value: Production
  - key: TEXT_GENERATE_TIMEOUT
    value: "120"
```

### Docker Configuration
```dockerfile
ENV TEXT_GENERATE_SERVICE_URL=https://your-text-service.com
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TEXT_GENERATE_TIMEOUT=120
```

### Kubernetes ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: api-gateway-config
data:
  TEXT_GENERATE_SERVICE_URL: "https://your-text-service.com"
  ASPNETCORE_ENVIRONMENT: "Production"
  TEXT_GENERATE_TIMEOUT: "120"
```

## 🔒 Security Best Practices

1. **Never commit `.env` files** - They contain sensitive information
2. **Use platform environment variables** for production secrets
3. **Rotate URLs and tokens** regularly
4. **Restrict CORS origins** in production (avoid `*`)
5. **Use HTTPS URLs** for all external services

## 🐛 Troubleshooting

### Service Connection Issues
```bash
# Check if service URL is accessible
curl -I $TEXT_GENERATE_SERVICE_URL/api/system/server-info

# Verify environment variables are loaded
# Check application logs for "Configuration Summary"
```

### Common Issues
- **Connection refused**: Check if `TEXT_GENERATE_SERVICE_URL` is correct
- **Timeout errors**: Increase `TEXT_GENERATE_TIMEOUT`
- **CORS errors**: Verify `CORS_ALLOWED_ORIGINS` includes your frontend domain
- **Health check failures**: Verify `TEXT_GENERATE_HEALTH_PATH` endpoint exists
