# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/HotelSearch.Api/HotelSearch.Api.csproj", "src/HotelSearch.Api/"]
COPY ["src/HotelSearch.Application/HotelSearch.Application.csproj", "src/HotelSearch.Application/"]
COPY ["src/HotelSearch.Domain/HotelSearch.Domain.csproj", "src/HotelSearch.Domain/"]
COPY ["src/HotelSearch.Infrastructure/HotelSearch.Infrastructure.csproj", "src/HotelSearch.Infrastructure/"]
RUN dotnet restore "src/HotelSearch.Api/HotelSearch.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/HotelSearch.Api"
RUN dotnet build "HotelSearch.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HotelSearch.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser:appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "HotelSearch.Api.dll"]
