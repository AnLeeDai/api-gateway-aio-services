#!/usr/bin/env bash
set -euo pipefail

# Runner for API Gateway: prod (published) or dev (hot reload if csproj exists)
# Usage:
#   scripts/run.sh                # prod, port 8080
#   scripts/run.sh prod           # prod, port 8080
#   PORT=9090 scripts/run.sh prod # prod, custom port
#   scripts/run.sh dev            # dev hot reload (if csproj), else fallback
#   PORT=5001 scripts/run.sh dev  # dev hot reload (custom port)

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
PROJECT_REL="src/ApiGateway/ApiGateway.csproj"
MODE="${1:-prod}"
DEFAULT_PORT="${PORT:-8080}"

# Function to check if port is available
check_port() {
	local port=$1
	! ss -tlnp | grep -q ":${port}"
}

# Find available port
find_available_port() {
	local start_port=$1
	for ((port=start_port; port<=start_port+100; port++)); do
		if check_port "$port"; then
			echo "$port"
			return 0
		fi
	done
	echo "Error: No available port found in range $start_port-$((start_port+100))" >&2
	exit 1
}

# Set PORT, auto-find if default is busy
if [[ -z "${PORT:-}" ]]; then
	if check_port "$DEFAULT_PORT"; then
		PORT="$DEFAULT_PORT"
	else
		echo "Warning: Port $DEFAULT_PORT is busy, finding alternative..."
		PORT=$(find_available_port $((DEFAULT_PORT+1)))
		echo "Using port $PORT instead"
	fi
fi

cd "$ROOT_DIR"

if [[ "$MODE" == "dev" ]]; then
	if [[ -f "$PROJECT_REL" ]]; then
		echo "[dev] Hot reload via dotnet watch on http://127.0.0.1:${PORT}"
		export ASPNETCORE_ENVIRONMENT=Development
		exec dotnet watch --project "$PROJECT_REL" run --no-launch-profile --urls "http://127.0.0.1:${PORT}"
	else
		echo "[dev] No csproj found. Fallback to published (no hot reload)"
		export ASPNETCORE_ENVIRONMENT=Development
		export ASPNETCORE_URLS="http://127.0.0.1:${PORT}"
		exec dotnet net9.0/ApiGateway.dll
	fi
else
	echo "[prod] Running published DLL on http://127.0.0.1:${PORT}"
	export ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Production}
	export ASPNETCORE_URLS="http://127.0.0.1:${PORT}"
	exec dotnet net9.0/ApiGateway.dll
fi
