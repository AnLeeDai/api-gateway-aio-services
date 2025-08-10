#!/bin/bash
# Build and test production Docker image locally

echo "Building production Docker image..."
docker build -t api-gateway-prod .

if [ $? -eq 0 ]; then
    echo "✅ Docker image built successfully!"
    
    echo "Starting container on port 10000..."
    docker run -d -p 10000:10000 --name api-gateway-test \
        -e ASPNETCORE_ENVIRONMENT=Production \
        -e ASPNETCORE_URLS=http://0.0.0.0:10000 \
        api-gateway-prod
    
    if [ $? -eq 0 ]; then
        echo "✅ Container started successfully!"
        echo "🌐 API Gateway available at: http://localhost:10000"
        echo "🔍 Health check: http://localhost:10000/health"
        echo "📊 System summary: http://localhost:10000/api/system/summary"
        
        echo ""
        echo "Waiting 10 seconds for container to start..."
        sleep 10
        
        echo "Testing health endpoint..."
        curl -f http://localhost:10000/health
        
        if [ $? -eq 0 ]; then
            echo ""
            echo "✅ Health check passed!"
        else
            echo ""
            echo "❌ Health check failed!"
        fi
        
        echo ""
        echo "To stop the test container:"
        echo "docker stop api-gateway-test && docker rm api-gateway-test"
    else
        echo "❌ Failed to start container!"
    fi
else
    echo "❌ Failed to build Docker image!"
fi
