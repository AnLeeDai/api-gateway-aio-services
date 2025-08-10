#!/bin/bash

# Start API Gateway Service
echo "🚀 Starting API Gateway Service..."
echo "📍 Service will run on: http://localhost:5257"
echo ""

# Check if port is available
if lsof -Pi :5257 -sTCP:LISTEN -t >/dev/null ; then
    echo "❌ Port 5257 is already in use!"
    echo "Please stop the process using port 5257 and try again."
    exit 1
fi

# Navigate to service directory
cd "$(dirname "$0")"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 9.0 SDK"
    exit 1
fi

# Restore packages if needed
if [ ! -d "bin" ]; then
    echo "📦 Restoring packages..."
    dotnet restore
fi

# Start the service
echo "✅ Starting API Gateway..."
dotnet run
