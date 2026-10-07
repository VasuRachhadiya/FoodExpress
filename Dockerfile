# ==============================================================================
# FoodExpress - Multi-stage Production Dockerfile for .NET 9.0
# Deployable to Render, Railway, Fly.io, Azure App Service, or Docker containers
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for efficient layer caching
COPY ["FoodExpress.sln", "./"]
COPY ["FoodExpress/FoodExpress.csproj", "FoodExpress/"]

RUN dotnet restore "FoodExpress/FoodExpress.csproj"

# Copy the rest of the application files
COPY FoodExpress/ FoodExpress/

# Build and publish release binaries
WORKDIR "/src/FoodExpress"
RUN dotnet publish "FoodExpress.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Cloud platform standard port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FoodExpress.dll"]
