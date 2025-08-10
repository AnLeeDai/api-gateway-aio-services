# Dockerfile for production deployment on Render
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Render will automatically expose port through PORT environment variable
# Debug: Print environment info
RUN echo "🔍 Base image setup complete"

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

# Don't set ASPNETCORE_URLS here - let Program.cs handle it from Render's PORT environment variable
# Create a non-root user
RUN addgroup --system --gid 1001 dotnet
RUN adduser --system --uid 1001 --gid 1001 dotnet

# Change ownership of the app directory
RUN chown -R dotnet:dotnet /app

USER dotnet

# Expose port for documentation (Render will override this)
EXPOSE 10000

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
