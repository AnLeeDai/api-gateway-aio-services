#!/bin/bash

# API Gateway Port Configuration
API_GATEWAY_PORT=5257
TEXT_GENERATE_PORT=8000

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

check_port() {
    local port=$1
    local service_name=$2
    
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${RED}❌ Error: Port $port is already in use!${NC}"
        echo -e "${YELLOW}Service: $service_name${NC}"
        echo -e "${YELLOW}Process using port $port:${NC}"
        lsof -Pi :$port -sTCP:LISTEN
        echo ""
        echo -e "${RED}🚫 CANNOT START $service_name - PORT $port IS OCCUPIED${NC}"
        echo -e "${YELLOW}Please stop the process using port $port or wait for it to be available.${NC}"
        echo -e "${YELLOW}Do not change to another port - this service must run on port $port.${NC}"
        return 1
    else
        echo -e "${GREEN}✅ Port $port is available for $service_name${NC}"
        return 0
    fi
}

echo "🔍 Checking required ports for services..."
echo "================================================="

# Check API Gateway port
check_port $API_GATEWAY_PORT "API Gateway"
api_gateway_status=$?

echo ""

# Check Text Generate Service port
check_port $TEXT_GENERATE_PORT "Text Generate Service"
text_generate_status=$?

echo ""
echo "================================================="

if [ $api_gateway_status -eq 0 ] && [ $text_generate_status -eq 0 ]; then
    echo -e "${GREEN}🎉 All ports are available! Services can be started.${NC}"
    exit 0
else
    echo -e "${RED}💥 Some ports are occupied! Cannot start services.${NC}"
    echo -e "${YELLOW}Required ports:${NC}"
    echo -e "  • API Gateway: $API_GATEWAY_PORT"
    echo -e "  • Text Generate Service: $TEXT_GENERATE_PORT"
    echo ""
    echo -e "${YELLOW}These ports are FIXED and cannot be changed.${NC}"
    exit 1
fi
