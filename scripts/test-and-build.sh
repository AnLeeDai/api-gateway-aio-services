#!/bin/bash

echo "Building and testing API Gateway..."
echo "==================================="

# Build the project
echo "1. Building project..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo "✅ Build successful!"

# Run the application in background
echo ""
echo "2. Starting application..."
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://localhost:5000

dotnet run --configuration Release --no-build &
APP_PID=$!

echo "Application started with PID: $APP_PID"
echo "Waiting for application to start..."
sleep 10

# Test endpoints
echo ""
echo "3. Testing endpoints..."

# Test health endpoint
echo "Testing /health endpoint:"
curl -s -w "Status: %{http_code}, Time: %{time_total}s\n" http://localhost:5000/health

echo ""
echo "Testing /health/text-generate endpoint:"
curl -s -w "Status: %{http_code}, Time: %{time_total}s\n" http://localhost:5000/health/text-generate

echo ""
echo "Testing root endpoint:"
curl -s -w "Status: %{http_code}, Time: %{time_total}s\n" http://localhost:5000/ | head -c 200

# Cleanup
echo ""
echo "4. Stopping application..."
kill $APP_PID
wait $APP_PID 2>/dev/null

echo ""
echo "==================================="
echo "Testing completed!"
echo ""
echo "If tests pass, you can deploy to Render by pushing to main branch:"
echo "git add ."
echo "git commit -m 'Improve text-generate service health checks and timeout handling'"
echo "git push origin main"
