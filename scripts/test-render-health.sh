#!/bin/bash

# Test health check for Render deployment
# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default URL (can be overridden)
BASE_URL=${1:-"https://api-gateway-aio.onrender.com"}

echo -e "${BLUE}🔍 Testing Render deployment health...${NC}"
echo -e "${BLUE}📍 Base URL: $BASE_URL${NC}"
echo "================================================="

# Test basic health endpoint
echo -e "${YELLOW}Testing /health endpoint...${NC}"
HEALTH_RESPONSE=$(curl -s -w "%{http_code}" -o /tmp/health_response.json "$BASE_URL/health" 2>/dev/null)
HTTP_CODE="${HEALTH_RESPONSE: -3}"

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ Health check passed (HTTP $HTTP_CODE)${NC}"
    echo -e "${BLUE}Response:${NC}"
    cat /tmp/health_response.json | jq . 2>/dev/null || cat /tmp/health_response.json
    echo ""
else
    echo -e "${RED}❌ Health check failed (HTTP $HTTP_CODE)${NC}"
    if [ -f /tmp/health_response.json ]; then
        echo -e "${RED}Response:${NC}"
        cat /tmp/health_response.json
        echo ""
    fi
fi

# Test system info endpoint
echo -e "${YELLOW}Testing /api/system/server-info endpoint...${NC}"
SYSTEM_RESPONSE=$(curl -s -w "%{http_code}" -o /tmp/system_response.json "$BASE_URL/api/system/server-info" 2>/dev/null)
HTTP_CODE="${SYSTEM_RESPONSE: -3}"

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ System info check passed (HTTP $HTTP_CODE)${NC}"
    echo -e "${BLUE}Response:${NC}"
    cat /tmp/system_response.json | jq . 2>/dev/null || cat /tmp/system_response.json
    echo ""
else
    echo -e "${RED}❌ System info check failed (HTTP $HTTP_CODE)${NC}"
    if [ -f /tmp/system_response.json ]; then
        echo -e "${RED}Response:${NC}"
        cat /tmp/system_response.json
        echo ""
    fi
fi

# Test system health endpoint  
echo -e "${YELLOW}Testing /api/system/health endpoint...${NC}"
SYSTEM_HEALTH_RESPONSE=$(curl -s -w "%{http_code}" -o /tmp/system_health_response.json "$BASE_URL/api/system/health" 2>/dev/null)
HTTP_CODE="${SYSTEM_HEALTH_RESPONSE: -3}"

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ System health check passed (HTTP $HTTP_CODE)${NC}"
    echo -e "${BLUE}Response:${NC}"
    cat /tmp/system_health_response.json | jq . 2>/dev/null || cat /tmp/system_health_response.json
    echo ""
else
    echo -e "${RED}❌ System health check failed (HTTP $HTTP_CODE)${NC}"
    if [ -f /tmp/system_health_response.json ]; then
        echo -e "${RED}Response:${NC}"
        cat /tmp/system_health_response.json
        echo ""
    fi
fi

echo "================================================="
echo -e "${BLUE}🏁 Health check testing complete!${NC}"

# Cleanup
rm -f /tmp/health_response.json /tmp/system_response.json /tmp/system_health_response.json
