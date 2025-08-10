# Troubleshooting Guide - Text Generate Service

## Common Issues and Solutions

### 1. "Connection refused (127.0.0.1:8000)" Error

**Symptoms:**
- Service status shows "unreachable"
- Error message: "Connection refused (127.0.0.1:8000)"

**Root Cause:**
- This error typically occurs when the health check is using localhost configuration instead of production URLs
- Can also happen during Render "cold starts" on free tier

**Solutions:**

#### A. Verify Environment Configuration
```bash
# Check if ASPNETCORE_ENVIRONMENT is set correctly
echo $ASPNETCORE_ENVIRONMENT

# Should be "Production" on Render
```

#### B. Test Direct Connection
```bash
# Test text-generate service directly
curl -I https://text-generate-services.onrender.com/api/system/server-info

# Should return HTTP 200
```

#### C. Check Configuration Files
- **Development**: Uses `http://127.0.0.1:8000`
- **Production**: Uses `https://text-generate-services.onrender.com`

### 2. Timeout Issues

**Symptoms:**
- Service status shows "timeout"
- Long response times (>30s)

**Solutions:**

#### A. Increased Timeouts (Applied in Latest Version)
- HttpClient timeout: 120 seconds
- Health check timeout: 30 seconds
- Health check interval: 60 seconds

#### B. Retry Logic (Applied in Latest Version)
- 3 retry attempts
- Progressive delay: 2s, 4s, 6s
- Detailed logging for troubleshooting

### 3. Cold Start Issues (Render Free Tier)

**Symptoms:**
- First request takes very long
- Service becomes "unreachable" after inactivity
- Subsequent requests work fine

**Solutions:**

#### A. Warm-up Strategy
- Health checks every 60 seconds help keep service warm
- Consider upgrading to paid Render plan for better reliability

#### B. Graceful Degradation
- API Gateway continues to work even if text-generate is temporarily unavailable
- Status endpoint shows detailed information about service health

### 4. Testing and Debugging

#### Test Scripts
```bash
# Test text-generate service connection
./scripts/test-text-generate-connection.sh

# Build and test locally  
./scripts/test-and-build.sh
```

#### Useful Endpoints
- `/health` - Overall health check
- `/health/text-generate` - Specific text-generate service health
- `/` - System summary with service status

#### Debug Information
Each health check response includes:
- Response time
- Number of attempts used
- Detailed error messages
- Timestamp of last check

### 5. Configuration Reference

#### Production Settings (`appsettings.Production.json`)
```json
{
  "TextGenerateService": {
    "BaseUrl": "https://text-generate-services.onrender.com",
    "Timeout": 120,
    "RetryCount": 3
  },
  "ReverseProxy": {
    "Clusters": {
      "cluster-text-generate": {
        "Destinations": {
          "primary": { "Address": "https://text-generate-services.onrender.com/" }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:01:00",
            "Timeout": "00:00:30",
            "Policy": "ConsecutiveFailures",
            "Path": "/api/system/server-info"
          }
        }
      }
    }
  }
}
```

### 6. Monitoring and Alerts

#### Key Metrics to Monitor
- Response time
- Success rate
- Retry attempts
- Health check status

#### Log Messages to Watch
- "Text-generate service health check successful"
- "HTTP request failed on attempt"
- "Request timeout on attempt"

### 7. When to Redeploy

Redeploy if you see:
- Persistent "unreachable" status after configuration changes
- Timeout values seem insufficient
- New retry logic is needed

#### Deployment Command
```bash
git add .
git commit -m "Fix text-generate service connection issues"
git push origin main
```

Render will automatically redeploy when you push to the main branch.
