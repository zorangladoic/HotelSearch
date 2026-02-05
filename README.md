# Hotel Search API

This is JSON REST web service for hotel search built with .NET 10, Clean Architecture, and DDD principles.

## Features

- **CRUD Operations**: Create, Read, Update, Delete hotels
- **Search**: Find hotels sorted by price and distance from your location
- **Pagination**: Support for paginated search results
- **JWT Authentication**: Secure API with token-based authentication

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

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/hotels` | Create a new hotel |
| GET | `/api/v1/hotels/{id}` | Get hotel by ID |
| GET | `/api/v1/hotels` | Get all hotels |
| PUT | `/api/v1/hotels/{id}` | Update a hotel |
| DELETE | `/api/v1/hotels/{id}` | Delete a hotel |

### Search

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/search?latitude={lat}&longitude={lon}&page={page}&pageSize={size}` | Search hotels |

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/token` | Generate JWT token |

## Implementation Status

- Project Setup & Domain Layer
- Application Layer (CQRS Foundation)
- Application Layer (Commands & Queries)
- Infrastructure Layer
- API Layer (CRUD)
- API Layer (Search & Caching)

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

## License

This project is created by zoran.gladoic@gmail.com, and is used for demonstration purposes.
