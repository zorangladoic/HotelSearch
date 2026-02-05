# Hotel Search API

This is JSON REST web service for hotel search built with .NET 10, Clean Architecture, and DDD principles.

## Features

- **CRUD Operations**: Create, Read, Update, Delete hotels
- **Search**: Find hotels sorted by price and distance from your location
- **Pagination**: Support for paginated search results
- **JWT Authentication**: Secure API with token-based authentication
- **Role-Based Authorization**: Admin role required for write operations

## Technology Stack

- **.NET 10**
- **ASP.NET Core Web API**
- **Clean Architecture**
- **CQRS Pattern** (MediatR-ready, simple custom implementation)
- **FluentValidation**
- **Polly** (resilience)
- **Serilog** (structured logging)
- **xUnit** (testing)

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
└── HotelSearch.sln
```

## Getting Started

### Prerequisites

- .NET 10 SDK

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

## Implementation Status

- Project Setup & Domain Layer
- Application Layer (CQRS Foundation)
- Application Layer (Commands & Queries)
- Infrastructure Layer
- API Layer (CRUD)
- API Layer (Search & Caching)
- Security (JWT Authentication)

## Domain Model

### Hotel Entity
- `Id` (Guid) - Unique identifier
- `Name` (string) - Hotel name with max. 200 chars)
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

**Note**: In production, use environment variables or a secrets manager for the SecretKey.

## License

This project is created by zoran.gladoic@gmail.com, and is used for demonstration purposes.
