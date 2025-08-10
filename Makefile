# Makefile: run production from published net9.0 or dev hot reload (if csproj exists)

PORT ?= 8080
ENV ?= Production

.PHONY: run dev docker

run:
	@echo "Running production from net9.0 on port $(PORT) (ENV=$(ENV))"
	ASPNETCORE_ENVIRONMENT=$(ENV) ASPNETCORE_URLS=http://127.0.0.1:$(PORT) dotnet net9.0/ApiGateway.dll

dev:
	@echo "Starting dev (hot reload if csproj exists)..."
	@if [ -f src/ApiGateway/ApiGateway.csproj ]; then \
		echo "Found csproj. Running dotnet watch..."; \
		ASPNETCORE_ENVIRONMENT=Development dotnet watch --project src/ApiGateway/ApiGateway.csproj run --no-launch-profile --urls http://127.0.0.1:$(PORT); \
	else \
		echo "No csproj found. Fallback to published run (no hot reload)."; \
		ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:$(PORT) dotnet net9.0/ApiGateway.dll; \
	fi

docker:
	docker compose up --build -d
