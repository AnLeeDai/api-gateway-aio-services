# API Gateway Service - Clean Build System
.PHONY: help build-dev run-dev build-prod run-prod test clean docker docker-dev

# Default environment variables
ENV ?= Development
PORT ?= 5257

help: ## Show this help message
	@echo "API Gateway Service - Available Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

# Development Commands
build-dev: ## Build the project for development
	@echo "🔨 Building development version..."
	dotnet build -c Debug

run-dev: ## Run the service in development mode
	@echo "🚀 Starting development server..."
	@echo "📍 Loading environment from .env file"
	dotnet run --environment Development

watch-dev: ## Run with hot reload for development
	@echo "🔄 Starting development server with hot reload..."
	dotnet watch run --environment Development

# Production Commands
build-prod: ## Build the project for production
	@echo "🔨 Building production version..."
	dotnet build -c Release

run-prod: ## Run the service in production mode
	@echo "🚀 Starting production server..."
	@echo "📍 Loading environment from .env.production file"
	dotnet run -c Release --environment Production

# Testing Commands
test: ## Run all tests
	@echo "🧪 Running tests..."
	dotnet test

# Maintenance Commands
clean: ## Clean build artifacts
	@echo "🧹 Cleaning build artifacts..."
	dotnet clean
	rm -rf bin obj

restore: ## Restore NuGet packages
	@echo "📦 Restoring packages..."
	dotnet restore

# Docker Commands
docker: ## Build and run with Docker Compose
	@echo "🐳 Building and running with Docker..."
	docker-compose up --build

docker-dev: ## Run development environment with Docker
	@echo "🐳 Running development environment..."
	docker-compose -f docker-compose.yml -f docker-compose.override.yml up

docker-prod: ## Run production environment with Docker
	@echo "🐳 Running production environment..."
	docker-compose -f docker-compose.prod.yml up --build

# Environment Commands
setup-env: ## Copy environment template
	@echo "📋 Setting up environment configuration..."
	@if [ ! -f .env ]; then \
		cp .env.example .env; \
		echo "✅ Created .env from .env.example"; \
		echo "📝 Please edit .env file with your configuration"; \
	else \
		echo "⚠️  .env file already exists"; \
	fi

check-env: ## Validate environment configuration
	@echo "🔍 Checking environment configuration..."
	@if [ -f .env ]; then \
		echo "✅ .env file exists"; \
	else \
		echo "❌ .env file missing - run 'make setup-env'"; \
	fi
