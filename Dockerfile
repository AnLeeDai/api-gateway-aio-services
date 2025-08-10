# Production Dockerfile - Uses Environment Variables for Configuration
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ApiGateway.csproj", "."]
RUN dotnet restore "ApiGateway.csproj"
COPY . .
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy environment configuration
COPY .env.production .env

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 dotnet && \
    chown -R dotnet:dotnet /app

USER dotnet

# Port is set via environment variables
EXPOSE 10000

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
