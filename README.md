# API Gateway Service

A clean and simple API Gateway service built with .NET 9 and YARP (Yet Another Reverse Proxy).

## Features

- 🚀 High-performance reverse proxy
- 🔧 Environment-based configuration
- 🏥 Health checks and monitoring
- 🔒 Security best practices
- 📊 System metrics and monitoring

## Quick Start

### Development

1. **Setup environment**:
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

2. **Run the service**:
   ```bash
   dotnet run
   ```

3. **Access the API**:
   - API: `http://localhost:5257`
   - Swagger: `http://localhost:5257/swagger`

### Production

1. **Build and run with Docker**:
   ```bash
   docker build -t api-gateway .
   docker run -p 10000:10000 api-gateway
   ```

2. **Or deploy to cloud platforms** (Render, Azure, AWS, etc.)

## Configuration

All configuration is managed through environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment mode | `Development` |
| `PORT` | Server port | `5257` (dev), `10000` (prod) |
| `TEXT_GENERATE_BASE_URL` | Backend service URL | `http://127.0.0.1:8000` |
| `TEXT_GENERATE_TIMEOUT` | Request timeout (seconds) | `30` (dev), `120` (prod) |
| `HEALTH_CHECK_INTERVAL` | Health check frequency | `00:00:30` |

See `.env.example` for all available options.

## API Endpoints

- `GET /api/system/server-info` - System information
- `GET /health` - Health check (production only)
- `/swagger` - API documentation (development only)

## Project Structure

```
├── Configuration/          # Environment configuration
├── Controllers/            # API controllers
├── Filters/               # Request/response filters
├── Middleware/            # Custom middleware
├── Models/                # Data models
├── Services/              # Business services
├── .env.example           # Environment template
└── Program.cs             # Application entry point
```

## Development Scripts

```bash
# Build and test
make build-dev

# Run development server
make run-dev

# Run production build
make build-prod
```

## Contributing

1. Fork the repository
2. Create your feature branch
3. Make changes following clean code principles
4. Test your changes
5. Submit a pull request

## License

MIT License
