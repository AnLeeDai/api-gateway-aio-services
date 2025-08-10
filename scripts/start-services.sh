#!/bin/bash

echo "🚀 Starting API Gateway and Text Generate Services..."
echo "================================================="
echo "⚠️  IMPORTANT: Services will run on FIXED ports:"
echo "   • API Gateway: 5257 (FIXED - cannot be changed)"
echo "   • Text Generate Service: 8000 (FIXED - cannot be changed)"
echo "================================================="

# Check ports first
echo "🔍 Checking if required ports are available..."
if ! ./scripts/check-port.sh; then
    echo ""
    echo "💥 Cannot start services - required ports are occupied!"
    exit 1
fi

echo ""

# Check if required directories exist
if [ ! -d "../text-generate-services" ]; then
    echo "❌ Error: text-generate-services directory not found!"
    echo "Please ensure the text-generate-services is in the correct path."
    exit 1
fi

if [ ! -d "./net9.0" ]; then
    echo "❌ Error: net9.0 directory not found!"
    echo "Please build the API Gateway project first."
    exit 1
fi

# Stop any existing containers
echo "🛑 Stopping existing containers..."
docker-compose down

# Build and start services
echo "🔨 Building and starting services..."
docker-compose up --build -d

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 10

# Check health of services
echo "🏥 Checking service health..."

echo "Checking API Gateway..."
if curl -f http://localhost:8080/health >/dev/null 2>&1; then
    echo "✅ API Gateway is healthy"
else
    echo "⚠️ API Gateway health check failed"
fi

echo "Checking Text Generate Service via Gateway..."
if curl -f http://localhost:8080/api/text-generate/health >/dev/null 2>&1; then
    echo "✅ Text Generate Service is healthy"
else
    echo "⚠️ Text Generate Service health check failed"
fi

echo ""
echo "🎉 Setup completed!"
echo ""
echo "📚 Available endpoints:"
echo "  Gateway Health:     http://localhost:5257/health"
echo "  Gateway Root:       http://localhost:5257/"
echo "  Server Info:        http://localhost:5257/api/text-generate/system/server-info"
echo "  List Files:         http://localhost:5257/api/text-generate/system/list"
echo "  Service Health:     http://localhost:5257/api/text-generate/health"
echo ""
echo "🔗 Direct routes (via YARP):"
echo "  System Info:        http://localhost:5257/api/system/server-info"
echo "  List Files:         http://localhost:5257/api/system/list"
echo "  Generate Bank Bill: http://localhost:5257/api/bank-bill/generate"
echo ""
echo "⚠️  FIXED PORTS - DO NOT CHANGE:"
echo "  API Gateway: 5257"
echo "  Text Generate Service: 8000"
echo ""
echo "📋 To view logs:"
echo "  docker-compose logs -f api-gateway"
echo "  docker-compose logs -f text-generate-app"
echo ""
echo "🛑 To stop services:"
echo "  docker-compose down"
