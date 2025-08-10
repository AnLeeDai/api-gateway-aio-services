#!/bin/bash

echo "Testing text-generate service connection..."
echo "============================================="

# Test direct connection to text-generate service
echo "1. Testing direct connection to text-generate service:"
curl -s -w "Response time: %{time_total}s\nHTTP code: %{http_code}\n" \
     https://text-generate-services.onrender.com/api/system/server-info

echo ""
echo "2. Testing with multiple attempts (simulating retry logic):"
for i in {1..3}; do
    echo "Attempt $i:"
    start_time=$(date +%s.%N)
    response=$(curl -s -w "%{http_code}" https://text-generate-services.onrender.com/api/system/server-info)
    end_time=$(date +%s.%N)
    duration=$(echo "$end_time - $start_time" | bc)
    http_code="${response: -3}"
    
    echo "  HTTP Code: $http_code"
    echo "  Response Time: ${duration}s"
    
    if [ "$http_code" = "200" ]; then
        echo "  Status: SUCCESS ✓"
        break
    else
        echo "  Status: FAILED ✗"
        if [ $i -lt 3 ]; then
            echo "  Waiting 2 seconds before retry..."
            sleep 2
        fi
    fi
    echo ""
done

echo ""
echo "3. Testing API Gateway locally (if running):"
if curl -s localhost:5257/health > /dev/null 2>&1; then
    echo "Local API Gateway is running, testing text-generate health check:"
    curl -s localhost:5257/health | head -c 500
else
    echo "Local API Gateway is not running"
fi

echo ""
echo "============================================="
echo "Test completed!"
