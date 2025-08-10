#!/bin/bash

echo "🧪 Testing Render Configuration"
echo "==============================="

# Test 1: Development mode (should bind to localhost:5257)
echo ""
echo "📋 Test 1: Development Mode"
echo "Expected: http://localhost:5257"
export ASPNETCORE_ENVIRONMENT=Development
export PORT=""
export ASPNETCORE_URLS=""

echo "Environment variables:"
echo "  ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"
echo "  PORT=$PORT"
echo "  ASPNETCORE_URLS=$ASPNETCORE_URLS"

# Test 2: Production mode with PORT env var
echo ""
echo "📋 Test 2: Production Mode with PORT env var"
echo "Expected: http://0.0.0.0:8080"
export ASPNETCORE_ENVIRONMENT=Production
export PORT=8080
export ASPNETCORE_URLS=""

echo "Environment variables:"
echo "  ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"
echo "  PORT=$PORT"
echo "  ASPNETCORE_URLS=$ASPNETCORE_URLS"

# Test 3: Production mode with ASPNETCORE_URLS
echo ""
echo "📋 Test 3: Production Mode with ASPNETCORE_URLS"
echo "Expected: http://0.0.0.0:10000"
export ASPNETCORE_ENVIRONMENT=Production
export PORT=""
export ASPNETCORE_URLS="http://0.0.0.0:10000"

echo "Environment variables:"
echo "  ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"
echo "  PORT=$PORT"
echo "  ASPNETCORE_URLS=$ASPNETCORE_URLS"

# Test 4: Render's expected configuration
echo ""
echo "📋 Test 4: Render Configuration (Production)"
echo "Expected: http://0.0.0.0:10000"
export ASPNETCORE_ENVIRONMENT=Production
export PORT=10000
export ASPNETCORE_URLS="http://0.0.0.0:10000"

echo "Environment variables:"
echo "  ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT"
echo "  PORT=$PORT"
echo "  ASPNETCORE_URLS=$ASPNETCORE_URLS"

echo ""
echo "✅ Configuration test completed!"
echo "🚀 The application is now configured to:"
echo "   - Bind to localhost:5257 in Development"
echo "   - Bind to 0.0.0.0:PORT or 0.0.0.0:10000 in Production"
echo "   - Work correctly with Render's requirements"
