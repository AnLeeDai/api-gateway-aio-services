# Environment Variables Configuration

## Development Setup

1. Copy the example environment file:
```bash
cp .env.example .env
```

2. Edit `.env` file with your local configuration:
```bash
# Text Generate Service URL
TEXT_GENERATE_SERVICE_URL=http://127.0.0.1:8000

# ASP.NET Core Environment
ASPNETCORE_ENVIRONMENT=Development

# Server URLs
ASPNETCORE_URLS=http://localhost:5257
```

## Production Deployment (Render)

Set these environment variables in your Render dashboard:

### Required Variables:
- `TEXT_GENERATE_SERVICE_URL`: URL of your text-generate service
  - Example: `https://text-generate-services.onrender.com`

### Optional Variables:
- `TEXT_GENERATE_TIMEOUT`: Request timeout in seconds (default: 120)
- `TEXT_GENERATE_RETRY_COUNT`: Number of retry attempts (default: 3)

## Environment Variable Priority

The application loads configuration in this order (highest to lowest priority):

1. **Environment Variables** (highest priority)
   - `TEXT_GENERATE_SERVICE_URL`
   - `TEXT_GENERATE_TIMEOUT` 
   - `TEXT_GENERATE_RETRY_COUNT`

2. **Configuration Files**
   - `appsettings.Production.json` (in production)
   - `appsettings.Development.json` (in development)
   - `appsettings.json` (base configuration)

3. **Fallback Values** (lowest priority)
   - BaseUrl: `http://127.0.0.1:8000`
   - Timeout: `120` seconds
   - RetryCount: `3` attempts

## Security Notes

- Never commit `.env` files to version control
- Use Render's environment variable dashboard for production secrets
- The `.env.example` file shows the expected format without sensitive values
