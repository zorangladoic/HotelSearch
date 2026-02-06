# Hotel Search API

A production-ready JSON REST web service for hotel search built with .NET 10, Clean Architecture, and Domain-Driven Design principles.

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Search Algorithm](#search-algorithm)
- [Authentication & Authorization](#authentication--authorization)
- [Database Readiness](#database-readiness)
- [Deployment](#deployment)
- [Configuration](#configuration)
- [Testing](#testing)
- [Task](#task)

---

## Features

- **CRUD Operations**: Create, Read, Update, Delete hotels with full validation
- **Proximity Search**: Find hotels sorted by combined price and distance score
- **Pagination**: Efficient paginated search results
- **Output Caching**: Cached search results with automatic invalidation on data changes
- **JWT Authentication**: Industry-standard token-based authentication
- **Role-Based Authorization**: Admin role required for write operations
- **Health Checks**: Liveness and readiness endpoints for Kubernetes/container orchestration
- **Structured Logging**: Serilog with correlation IDs for distributed tracing
- **Docker Support**: Multi-stage Dockerfile optimized for production
- **CI/CD Ready**: GitHub Actions workflow for build, test, and deploy

---

## Technology Stack

| Category | Technology |
|----------|------------|
| Framework  | .NET 10, ASP.NET Core Web API |
| Architecture | Clean Architecture, DDD, CQRS |
| Validation | FluentValidation |
| Resilience | Polly (configured for future use) |
| Logging | Serilog (structured JSON) |
| Caching | ASP.NET Output Caching |
| Authentication | JWT Bearer Tokens |
| Testing | xUnit, FluentAssertions |
| Containerization | Docker (multi-stage build) |
| CI/CD | GitHub  Actions |

---

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                       │
│                     (HotelSearch.Api)                       │
│         Controllers, Middleware, Request/Response DTOs      |
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION LAYER                        │
│                 (HotelSearch.Application)                   │
│     CQRS Commands/Queries, Handlers, Validators, DTOs       │
├─────────────────────────────────────────────────────────────┤
│                      DOMAIN LAYER                           │
│                   (HotelSearch.Domain)                      │
│         Entities, Value Objects, Domain Exceptions          │
├─────────────────────────────────────────────────────────────┤
│                   INFRASTRUCTURE LAYER                      │
│                (HotelSearch.Infrastructure)                 │
│       Repositories, External Services, Data Access          │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

**Domain Layer** (innermost, no dependencies)
- `Hotel` entity with business rules and validation
- `GeoLocation` value object with Haversine distance calculation
- `GeoConstants` centralized geographic constants
- `ISystemClock` abstraction for testable time operations
- Domain exceptions (`HotelNotFoundException`, etc.)

**Application Layer** (depends only on Domain)
- CQRS pattern with Commands (write) and Queries (read)
- `IDispatcher` interface (MediatR-ready, custom implementation)
- FluentValidation validators for all requests
- `IHotelSearchService` interface for search business logic
- DTOs for data transfer between layers

**Infrastructure Layer** (implements Application interfaces)
- `InMemoryHotelRepository` - Thread-safe in-memory storage
- `HotelSearchService` - Bounding box + Haversine search implementation
- Polly resilience policies (ready for external services)

**Presentation Layer** (API endpoints)
- REST controllers with proper HTTP semantics
- Global exception middleware with correlation IDs
- JWT authentication and role-based authorization
- Output caching with cache invalidation
- Health check endpoints

### CQRS Pattern

The application uses a lightweight CQRS implementation:

```csharp
// Commands (write operations)
CreateHotelCommand → CreateHotelCommandHandler → HotelDto
UpdateHotelCommand → UpdateHotelCommandHandler → HotelDto
DeleteHotelCommand → DeleteHotelCommandHandler → Unit

// Queries (read operations)
GetHotelByIdQuery → GetHotelByIdQueryHandler → HotelDto?
GetAllHotelsQuery → GetAllHotelsQueryHandler → IReadOnlyList<HotelDto>
SearchHotelsQuery → SearchHotelsQueryHandler → PagedResultDto<HotelSearchResultDto>
```

The `IDispatcher` interface mirrors MediatR, enabling easy migration if needed:
```csharp
public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct);
}
```

---

## Project Structure

```
HotelSearch/
├── src/
│   ├── HotelSearch.Api/                    # Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs           # JWT token generation
│   │   │   ├── HotelsController.cs         # CRUD + cache invalidation
│   │   │   └── SearchController.cs         # Proximity search
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs  # DI registration
│   │   ├── Middleware/
│   │   │   ├── CorrelationIdMiddleware.cs  # Request tracing
│   │   │   └── ExceptionMiddleware.cs      # Global error handling
│   │   ├── Models/
│   │   │   └── ErrorResponse.cs            # Standardized error format
│   │   └── Program.cs                      # Application entry point
│   │
│   ├── HotelSearch.Application/            # Application Layer
│   │   ├── Common/
│   │   │   ├── Behaviors/                  # Validation, logging pipelines
│   │   │   ├── Interfaces/                 # CQRS interfaces
│   │   │   ├── Validators/                 # Shared validation rules
│   │   │   └── Dispatcher.cs               # CQRS dispatcher
│   │   ├── DTOs/                           # Data transfer objects
│   │   ├── Hotels/
│   │   │   ├── Commands/                   # Create, Update, Delete
│   │   │   └── Queries/                    # GetById, GetAll, Search
│   │   ├── Interfaces/                     # Repository, service interfaces
│   │   └── Mappings/
│   │       └── HotelMappingExtensions.cs   # Entity-to-DTO mappings
│   │
│   ├── HotelSearch.Domain/                 # Domain Layer
│   │   ├── Common/
│   │   │   ├── Entity.cs                   # Base entity class
│   │   │   ├── GeoConstants.cs             # All geographic constants
│   │   │   └── ISystemClock.cs             # Time abstraction
│   │   ├── Entities/
│   │   │   └── Hotel.cs                    # Hotel aggregate root
│   │   ├── Exceptions/                     # Domain-specific exceptions
│   │   └── ValueObjects/
│   │       └── GeoLocation.cs              # Immutable coordinates
│   │
│   └── HotelSearch.Infrastructure/         # Infrastructure Layer
│       ├── Caching/
│       │   └── CacheKeys.cs                # Cache key constants
│       ├── Extensions/
│       │   └── InfrastructureServiceExtensions.cs  # DI registration
│       ├── Repositories/
│       │   └── InMemoryHotelRepository.cs  # Thread-safe storage
│       ├── Resilience/
│       │   └── PollyPolicies.cs            # Retry, circuit breaker
│       └── Services/
│           └── HotelSearchService.cs       # Search algorithm
│
├── tests/
│   ├── HotelSearch.IntegrationTests/       # 19 integration tests
│   └── HotelSearch.UnitTests/              # 51 unit tests
│
├── postman/                                # API testing collection
│   ├── HotelSearch.postman_collection.json
│   └── HotelSearch.postman_environment.json
│
├── .github/workflows/                      # CI/CD pipelines
├── Dockerfile                              # Multi-stage container build
├── global.json                             # .NET SDK version
├── HotelSearch.sln                         # Solution file
└── VERSION.md                              # Changelog
```

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (optional)

### Quick Start

```bash
# Clone and build
git clone <repository-url>
cd HotelSearch
dotnet build

# Run the API
dotnet run --project src/HotelSearch.Api

# Run tests
dotnet test
```

The API will be available at:
- HTTP: `http://localhost:5236`
- HTTPS: `https://localhost:7094`
- Swagger: `http://localhost:5236/swagger`

### Docker

```bash
# Build image
docker build -t hotel-search-api .

# Run container
docker run -d -p 8080:8080 --name hotel-api hotel-search-api

# With custom JWT secret
docker run -d -p 8080:8080 \
  -e Jwt__SecretKey="YourProductionSecretKey" \
  --name hotel-api hotel-search-api
```

---

## API Endpoints

### Hotels (CRUD)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/hotels` | Create hotel | Admin |
| `GET` | `/api/v1/hotels/{id}` | Get by ID | Public |
| `GET` | `/api/v1/hotels` | Get all | Public |
| `PUT` | `/api/v1/hotels/{id}` | Update hotel | Admin |
| `DELETE` | `/api/v1/hotels/{id}` | Delete hotel | Admin |

### Search

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/search?latitude=45.8&longitude=15.9&page=1&pageSize=10` | Search hotels | Public |

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/auth/token` | Get JWT token |

### Health Checks

| Endpoint | Description |
|----------|-------------|
| `/health` | Full health status |
| `/health/live` | Liveness probe |
| `/health/ready` | Readiness probe |

---

## Search Algorithm

The search endpoint returns hotels sorted by a **combined score** of price and distance.

### Two-Phase Algorithm

The search uses an optimized two-phase approach for performance:

#### Phase 1: Bounding Box Pre-Filter (Fast)

Before calculating expensive distances, we eliminate hotels that are definitely outside the search area using a geographic bounding box:

```
Bounding Box Calculation:
┌─────────────────────────────────────┐
│           MaxLat                     │
│    ┌─────────────────────┐          │
│    │                     │          │
│ MinLon    [User]      MaxLon       │
│    │                     │          │
│    └─────────────────────┘          │
│           MinLat                     │
└─────────────────────────────────────┘

deltaLat = radius / 111 km (constant)
deltaLon = radius / (111 * cos(latitude)) km (varies with latitude)
```

This is O(n) with simple comparisons - very fast.

#### Phase 2: Haversine Distance (Precise)

Only for hotels within the bounding box, we calculate the exact great-circle distance using the Haversine formula:

```
a = sin²(Δlat/2) + cos(lat₁) × cos(lat₂) × sin²(Δlon/2)
c = 2 × atan2(√a, √(1-a))
distance = EarthRadius × c
```

This is O(m) where m << n (hotels in bounding box).

### Sorting Score

Hotels are sorted by a combined normalized score:

```
score = (normalizedPrice × 0.5) + (normalizedDistance × 0.5)

Where:
- normalizedPrice = (price - minPrice) / (maxPrice - minPrice)
- normalizedDistance = (distance - minDistance) / (maxDistance - minDistance)
```

**Lower score = better** (cheaper and closer hotels rank higher).

### Geographic Constants

All constants are centralized in `GeoConstants.cs`:

| Constant | Value | Description |
|----------|-------|-------------|
| `EarthRadiusKm` | 6,371 km | Mean Earth radius |
| `KmPerDegreeLat` | 111 km | Constant everywhere |
| `MinLatitude` | -90° | South Pole |
| `MaxLatitude` | 90° | North Pole |
| `MinLongitude` | -180° | Antimeridian |
| `MaxLongitude` | 180° | Antimeridian |
| `PriceWeight` | 0.5 | Sorting weight |
| `DistanceWeight` | 0.5 | Sorting weight |

### Performance Characteristics

| Hotels | Without Optimization | With Bounding Box |
|--------|---------------------|-------------------|
| 1,000 | ~1,000 Haversine calcs | ~100 calcs |
| 10,000 | ~10,000 calcs | ~500 calcs |
| 100,000 | ~100,000 calcs | ~2,000 calcs |

---

## Authentication & Authorization

### JWT Token Authentication

The API uses JWT Bearer tokens for authentication:

```bash
# Get token (use "admin" username for Admin role)
curl -X POST http://localhost:5236/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "any"}'

# Response
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresInMinutes": 60
}
```

### Using the Token

```bash
curl -X POST http://localhost:5236/api/v1/hotels \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -d '{"name": "Grand Hotel", "pricePerNight": 150, "latitude": 45.8, "longitude": 15.9}'
```

### Role-Based Authorization

| Role | Permissions |
|------|-------------|
| **Admin** | Full CRUD access (username: "admin") |
| **User** | Read-only (GET endpoints, search) |
| **Anonymous** | Read-only (GET endpoints, search) |

### Authorization Configuration

```csharp
// In Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// On controller actions
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> Create(...) { }

[AllowAnonymous]
public async Task<IActionResult> Search(...) { }
```

---

## Database Readiness

The application is **designed for easy database integration**. The current in-memory implementation can be replaced with Entity Framework Core without changing any other layer.

### Current Implementation

```csharp
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken ct);
    Task<Hotel> AddAsync(Hotel hotel, CancellationToken ct);
    Task<Hotel> UpdateAsync(Hotel hotel, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}
```

### Adding Entity Framework Core

1. **Install packages**:
```bash
dotnet add src/HotelSearch.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/HotelSearch.Infrastructure package Microsoft.EntityFrameworkCore.Tools
```

2. **Create DbContext**:
```csharp
public class HotelDbContext : DbContext
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).HasMaxLength(200).IsRequired();
            entity.Property(h => h.PricePerNight).HasPrecision(18, 2);
            entity.OwnsOne(h => h.Location, location =>
            {
                location.Property(l => l.Latitude).HasColumnName("Latitude");
                location.Property(l => l.Longitude).HasColumnName("Longitude");
            });
        });
    }
}
```

3. **Implement EF Repository**:
```csharp
public class EfHotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Hotels.FindAsync([id], ct);

    // ... implement other methods
}
```

4. **Register in DI**:
```csharp
// Replace this line:
services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();

// With:
services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
services.AddScoped<IHotelRepository, EfHotelRepository>();
```

### Spatial Index Optimization

For production with large datasets, add a spatial search method:

```csharp
// Add to IHotelRepository
Task<IReadOnlyList<Hotel>> SearchNearbyAsync(
    double latitude,
    double longitude,
    double radiusKm,
    CancellationToken ct);

// SQL Server implementation using geography type
public async Task<IReadOnlyList<Hotel>> SearchNearbyAsync(...)
{
    var point = $"POINT({longitude} {latitude})";
    return await _context.Hotels
        .Where(h => h.Location.Distance(point) <= radiusKm * 1000)
        .ToListAsync(ct);
}
```

---

## Deployment

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Development |
| `Jwt__SecretKey` | JWT signing key | (from appsettings) |
| `Jwt__Issuer` | Token issuer | HotelSearchApi |
| `Jwt__Audience` | Token audience | HotelSearchApiClients |
| `Jwt__ExpirationMinutes` | Token lifetime | 60 |

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hotel-search-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: hotel-search-api
  template:
    metadata:
      labels:
        app: hotel-search-api
    spec:
      containers:
      - name: api
        image: hotel-search-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: hotel-search-secrets
              key: jwt-secret
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: hotel-search-api
spec:
  selector:
    app: hotel-search-api
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP
```

### Production Checklist

- [ ] Set strong JWT secret key (min 32 characters)
- [ ] Configure HTTPS/TLS termination
- [ ] Set up database connection (replace in-memory)
- [ ] Configure distributed caching (Redis) for multi-instance
- [ ] Set up monitoring and alerting
- [ ] Configure rate limiting for public endpoints
- [ ] Review and adjust resource limits

---

## Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Issuer": "HotelSearchApi",
    "Audience": "HotelSearchApiClients",
    "SecretKey": "YourSuperSecretKey_MustBeAtLeast32Characters!",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "OutputCache": {
    "SearchResultsDuration": 120
  }
}
```

### Validation Rules

| Field | Rule |
|-------|------|
| Hotel Name | Required, max 200 characters |
| Price | >= $0.01, <= $100,000,000 |
| Latitude | -90° to 90° |
| Longitude | -180° to 180° |
| Page | > 0 |
| PageSize | 1 to 100 |

---

## Testing

### Test Coverage

- **Unit Tests**: 51 tests covering domain, application, and infrastructure
- **Integration Tests**: 19 tests covering full API flows
- **Total**: 70 tests, all passing

### Running Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/HotelSearch.UnitTests
```

### Postman Collection

Import from `postman/` directory for manual API testing:
- `HotelSearch.postman_collection.json` - All endpoints with tests
- `HotelSearch.postman_environment.json` - Environment variables

---

## Task

Problem statement

You are required to develop a JSON REST web service for hotel search.
The service must have two API interfaces:

1. CRUD interface for hotel data management
Required hotel data includes:
- Hotel name
- Hotel price
- Hotel geo location

2. Search interface that returns the list of all hotels to the
user
Search parameter:
- My current geo location
Output: List of hotels
- For each hotel, return the name, the price, and the distance from my current
location
- The list should be ordered. Hotels that are cheaper and closer to my current
location should be positioned closer to the top of the list. Hotels that are more
expensive and further away should be positioned closer to the bottom of the list.
The search interface should return only the hotels prepared through the CRUD interface.
You are not required to use any persistent storage (database or similar), but the design of
the application should enable easy addition of the persistence layer afterwards. You’ll
score bonus points if the search interface supports paging.
Expected outcome

You should prepare a working proof-of-concept (PoC) solution for this assignment. At
Lemax, we work with the Microsoft .NET ecosystem, so a solution written in C# and based
on the .NET stack is preferred.
Demonstrating knowledge of clean architecture and domain-driven design principles is a
strong plus.

As part of the assignment, we also ask you to show how you leveraged AI tools (e.g.,
ChatGPT, GitHub Copilot) to accelerate development and improve the quality of your
solution.
Evaluation

Be sure to submit your solution before the agreed deadline. Submit it in any form you
think is most appropriate. After the submission, you’ll be asked to present the solution in
person. We’ll evaluate the solution based on the following criteria:

- Functionality – is the application functioning as expected? Are negative and corner cases
covered?
- Technical design – how well does the code follow relevant design principles (OOP, Design
patterns, SOLID, DRY…)? Is the code extensible and reusable?
- Technology – are proper tools and libraries leveraged where possible?
- Standards – is the API aligned with industry standards and guidelines (HTTP, REST…)?
- Coding style – is the coding style clean and consistent? How’s the variable naming?
- Source code organization – are source code files organized in a folder structure according
to industry best practices? Is the solution committed to a source code repository (GitHub,
Bitbucket, GitLab, etc.)?
- Performance – what data structures and algorithms are selected? What is the complexity of
the search functionality? Does it allow for scaling?
- Security – are secure coding practices used (defensive programming, input validation, etc.)?
Does the API implement authentication and authorization?
- Test coverage – does the solution include unit tests? Are the test cases documented? Is
test execution automated?
- Documentation – is the code self-documenting? Are code comments used, and for what
purpose? Does the solution include markdown documentation? How easy is it for the next
developer to take over this solution?
- Processes – does the solution include any elements of the CI/CD (build, package, test…)?
How much attention was given to the application logs? Are there any other aspects
implemented that would ease the usage of the application in a production environment
(monitoring, health checks, etc.)?
- Presentation skills – how is the solution presented? Are you able to present the solution to
both the technical and non-technical audience? How do you accept the feedback? How do
you answer questions (good ones and bad ones)?

---

## License

Created by zoran.gladoic@gmail.com for demonstration purposes.
