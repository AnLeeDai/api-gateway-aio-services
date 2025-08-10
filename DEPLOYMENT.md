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

1. **Connect Repository**: Connect your GitHub repository to Render
2. **Service Configuration**: 
   - Type: Web Service
   - Environment: Docker
   - Dockerfile Path: `./Dockerfile`
   - Port: 10000 (Render default)
3. **Environment Variables**:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ASPNETCORE_URLS=http://0.0.0.0:10000`

## Production Configuration

- **Text Generate Service**: `https://text-generate-services.onrender.com`
- **Health Check**: `/health` endpoint
- **Port**: 10000 (Render requirement)
- **Environment**: Production

## API Endpoints

- Health Check: `GET /health`
- System Summary: `GET /api/system/summary`
- Text Generate: `POST /api/text-generate/{action}`
- Bank Bill: `/api/bank-bill/{endpoint}`
- Proxy: `/api/proxy/{endpoint}`

## Configuration Files

- `Dockerfile`: Multi-stage build for production
- `appsettings.Production.json`: Production configuration with Render endpoints
- `docker-compose.prod.yml`: Production docker compose
- `render.yaml`: Render service configuration
