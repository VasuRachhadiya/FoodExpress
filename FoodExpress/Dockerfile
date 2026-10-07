# ==============================================================================
# FoodExpress - Dockerfile for deployment when Root Directory is set to FoodExpress
# ==============================================================================

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FoodExpress.csproj", "./"]
RUN dotnet restore "FoodExpress.csproj"

COPY . .
RUN dotnet publish "FoodExpress.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FoodExpress.dll"]
