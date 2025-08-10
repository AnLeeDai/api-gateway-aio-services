# API Gateway - Production Deployment

## Docker Build Commands

```bash
# Build production image
docker build -t api-gateway-prod .

# Run production container
docker run -d -p 80:80 --name api-gateway-prod \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://0.0.0.0:80 \
  api-gateway-prod

# Using docker-compose for production
docker-compose -f docker-compose.prod.yml up -d
```

## Render Deployment

The application is now **automatically configured** for Render deployment! 🚀

### Automatic Configuration
- **Development**: Binds to `localhost:5257` with port availability checking
- **Production**: Automatically binds to `0.0.0.0` with Render's requirements

### Render Setup Steps
1. **Connect Repository**: Connect your GitHub repository to Render
2. **Service Configuration**: 
   - Type: Web Service
   - Environment: Docker
   - Dockerfile Path: `./Dockerfile`
   - Branch: `main`
   - Health Check Path: `/health`
3. **Environment Variables** (automatically configured via `render.yaml`):
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ASPNETCORE_URLS=http://0.0.0.0:10000`
   - `PORT=10000`

### Configuration Behavior
The application intelligently detects the environment:
- **Development Mode**: 
  - Checks port 5257 availability
  - Binds to `http://localhost:5257`
  - Shows port conflict errors if needed
- **Production Mode** (Render):
  - Reads `PORT` environment variable (fallback: 10000)
  - Reads `ASPNETCORE_URLS` environment variable
  - Binds to `http://0.0.0.0:PORT` as required by Render
  - No port conflict checking (managed by Render)

## Production Configuration

- **Text Generate Service**: `https://text-generate-services.onrender.com`
- **Health Check**: `/health` endpoint
- **Text Generate Health**: `/health/text-generate` endpoint  
- **Port**: 10000 (Render requirement)
- **Environment**: Production
- **Timeout**: 120 seconds (increased for cold start handling)
- **Retry Logic**: 3 attempts with progressive delay
- **Health Check Interval**: 60 seconds (reduced frequency for stability)

## API Endpoints

- Health Check: `GET /health`
- Text Generate Health: `GET /health/text-generate`
- System Summary: `GET /api/system/summary`  
- Text Generate: `POST /api/text-generate/{action}`
- Bank Bill: `/api/bank-bill/{endpoint}`
- Proxy: `/api/proxy/{endpoint}`

## Testing Scripts

- `scripts/test-text-generate-connection.sh`: Test text-generate service connectivity
- `scripts/test-and-build.sh`: Build and test application locally

## Configuration Files

- `Dockerfile`: Multi-stage build for production
- `appsettings.Production.json`: Production configuration with Render endpoints
- `docker-compose.prod.yml`: Production docker compose
- `render.yaml`: Render service configuration
