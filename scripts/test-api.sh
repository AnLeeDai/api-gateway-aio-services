#!/bin/bash

echo "🧪 Testing API Gateway and Text Generate Services Integration..."

BASE_URL="http://localhost:8080"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    
    echo -e "${YELLOW}Testing: $description${NC}"
    echo "Endpoint: $method $endpoint"
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}" "$BASE_URL$endpoint")
    elif [ "$method" = "POST" ]; then
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X POST -H "Content-Type: application/json" -d "$data" "$BASE_URL$endpoint")
    elif [ "$method" = "DELETE" ]; then
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X DELETE "$BASE_URL$endpoint")
    fi
    
    http_code=$(echo "$response" | tail -n1 | cut -d: -f2)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✅ SUCCESS (HTTP $http_code)${NC}"
        echo "Response: $body" | jq . 2>/dev/null || echo "Response: $body"
    else
        echo -e "${RED}❌ FAILED (HTTP $http_code)${NC}"
        echo "Response: $body"
    fi
    echo "----------------------------------------"
}

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 5

# Test gateway health
test_endpoint "GET" "/health" "Gateway Health Check"

# Test gateway root
test_endpoint "GET" "/" "Gateway Root System Info"

# Test text-generate service via controller
test_endpoint "GET" "/api/text-generate/health" "Text Generate Service Health (via Controller)"
test_endpoint "GET" "/api/text-generate/system/server-info" "Text Generate Server Info (via Controller)"
test_endpoint "GET" "/api/text-generate/system/list" "Text Generate List Files (via Controller)"

# Test text-generate service via YARP reverse proxy
test_endpoint "GET" "/api/system/server-info" "Text Generate Server Info (via YARP)"
test_endpoint "GET" "/api/system/list" "Text Generate List Files (via YARP)"

# Test bank bill generation
bank_bill_data='{
    "template": "bank_statement",
    "data": {
        "account_number": "123456789",
        "account_holder": "John Doe",
        "bank_name": "Test Bank",
        "statement_period": "January 2024",
        "transactions": [
            {
                "date": "2024-01-01",
                "description": "Initial Deposit",
                "amount": 1000.00,
                "balance": 1000.00
            },
            {
                "date": "2024-01-15",
                "description": "ATM Withdrawal",
                "amount": -100.00,
                "balance": 900.00
            }
        ]
    },
    "format": "pdf",
    "options": {
        "page_size": "A4",
        "orientation": "portrait"
    }
}'

test_endpoint "POST" "/api/text-generate/bank-bill/generate" "Bank Bill Generation (via Controller)" "$bank_bill_data"
test_endpoint "POST" "/api/bank-bill/generate" "Bank Bill Generation (via YARP)" "$bank_bill_data"

echo ""
echo "🎯 Test Summary:"
echo "- All endpoints have been tested"
echo "- Check the results above for any failures"
echo ""
echo "📖 Additional manual tests you can run:"
echo "curl $BASE_URL/health"
echo "curl $BASE_URL/api/system/server-info"
echo "curl $BASE_URL/api/text-generate/health"
