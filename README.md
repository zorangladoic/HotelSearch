# Hotel Search API

This is JSON REST web service for hotel search built with .NET 10, Clean Architecture, and DDD principles.

## Features

- **CRUD Operations**: Create, Read, Update, Delete hotels
- **Search**: Find hotels sorted by price and distance from your location
- **Pagination**: Support for paginated search results
- **JWT Authentication**: Secure API with token-based authentication
- **Role-Based Authorization**: Admin role required for write operations
- **Health Checks**: Liveness and readiness endpoints for container orchestration
- **Structured Logging**: Serilog with correlation IDs
- **Docker Support**: Multi-stage Dockerfile for containerization
- **CI/CD**: GitHub Actions workflow for build, test, and deploy

## Technology Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Clean Architecture**
- **CQRS Pattern** (MediatR-ready, simple custom implementation)
- **FluentValidation**
- **Polly** (resilience)
- **Serilog** (structured logging)
- **xUnit** (testing)
- **Docker** (containerization)
- **GitHub Actions** (CI/CD)

## Project Structure

```
HotelSearch/
├── src/
│   ├── HotelSearch.Api/              # Presentation layer
│   ├── HotelSearch.Application/      # Application layer (CQRS)
│   ├── HotelSearch.Domain/           # Domain layer
│   └── HotelSearch.Infrastructure/   # Infrastructure layer
├── tests/
│   ├── HotelSearch.UnitTests/        # Unit tests
│   └── HotelSearch.IntegrationTests/ # Integration tests
├── .github/workflows/                # CI/CD pipelines
├── Dockerfile                        # Container definition
└── HotelSearch.sln
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (optional, for containerization)

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/HotelSearch.Api
```

### Test

```bash
dotnet test
```

### Docker

Build the Docker image:

```bash
docker build -t hotel-search-api .
```

Run the container:

```bash
docker run -d -p 8080:8080 --name hotel-api hotel-search-api
```

The API will be available at `http://localhost:8080`.

Run with environment variables (for production JWT secret):

```bash
docker run -d -p 8080:8080 \
  -e Jwt__SecretKey="YourProductionSecretKey" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  --name hotel-api hotel-search-api
```

View container logs:

```bash
docker logs -f hotel-api
```

Stop and remove the container:

```bash
docker stop hotel-api && docker rm hotel-api
```

#### Docker Compose (optional)

Create a `docker-compose.yml`:

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

Run with Docker Compose:

```bash
docker-compose up -d
```

## API Endpoints

### Hotels (CRUD)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/hotels` | Create a new hotel | Admin |
| GET | `/api/v1/hotels/{id}` | Get hotel by ID | Anonymous |
| GET | `/api/v1/hotels` | Get all hotels | Anonymous |
| PUT | `/api/v1/hotels/{id}` | Update a hotel | Admin |
| DELETE | `/api/v1/hotels/{id}` | Delete a hotel | Admin |

### Search

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/search?latitude={lat}&longitude={lon}&page={page}&pageSize={size}` | Search hotels | Anonymous |

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/token` | Generate JWT token |

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Full health status with all checks |
| `/health/live` | Liveness probe (is the app running?) |
| `/health/ready` | Readiness probe (is the app ready to receive traffic?) |

## Authentication

### Getting a Token

```bash
curl -X POST http://localhost:5000/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "any"}'
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresInMinutes": 60
}
```

### Using the Token

Include the token in the Authorization header:
```bash
curl -X POST http://localhost:5000/api/v1/hotels \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{"name": "Grand Hotel", "pricePerNight": 150.00, "latitude": 45.815, "longitude": 15.982}'
```

### Roles

- **Admin**: Full access to all CRUD operations (use username "admin")
- **User**: Read-only access to hotels and search

## Health Checks

Check application health:
```bash
curl http://localhost:5000/health
```

Response:
```json
{
  "status": "Healthy",
  "checks": [
    { "name": "self", "status": "Healthy", "description": "Application is running" },
    { "name": "ready", "status": "Healthy", "description": "Application is ready" }
  ],
  "duration": 0.5
}
```

## CI/CD

The project includes a GitHub Actions workflow that:
- Builds the solution
- Runs unit tests
- Runs integration tests
- Publishes artifacts (on master push)
- Builds Docker image (on master push)

## Implementation Status

- Project Setup & Domain Layer
- Application Layer (CQRS Foundation)
- Application Layer (Commands & Queries)
- Infrastructure Layer
- API Layer (CRUD)
- API Layer (Search & Caching)
- Security (JWT Authentication)
- Logging & Observability
- CI/CD & Final Polish

## Domain Model

### Hotel Entity
- `Id` (Guid) - Unique identifier
- `Name` (string) - Hotel name with max. 200 chars
- `PricePerNight` (decimal) - Price per night (> 0)
- `Location` (GeoLocation) - Latitude and longitude

### GeoLocation Value Object
- `Latitude` (double) - Range: -90 to 90
- `Longitude` (double) - Range: -180 to 180
- `DistanceTo()` - Haversine formula for distance calculation

## Security Configuration

JWT settings can be configured in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "HotelSearchApi",
    "Audience": "HotelSearchApiClients",
    "SecretKey": "YourSuperSecretKeyForJwtTokenGeneration_MustBeAtLeast32Characters!",
    "ExpirationMinutes": 60
  }
}
```

Generate a secure key: `openssl rand -base64 32`

**Note**: In production, use environment variables or a secrets manager for the SecretKey.

## Logging Configuration

Serilog is configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

Logs include:
- Request/response timing
- Correlation IDs for request tracing
- Structured JSON output

## License

This project is created by zoran.gladoic@gmail.com, and is used for demonstration purposes.
