# Version History

## [1.2.0] - 2025-02-06

### Fixed
- Docker container now correctly listens on port 8080 (added ASPNETCORE_URLS env variable)
- HTTPS redirection disabled in Production (container uses HTTP)
- Swagger disabled in Production environment
- Disabled output cache on `HotelsController` GET endpoints (`GetById`, `GetAll`) to prevent stale data after updates
- Fixed search cache invalidation by adding `.Tag("SearchResults")` to cache policy (cache eviction was not working)

### Changed
- `GeoLocation` value object:
  - Fixed equality/hash contract by quantizing coordinates used in `GetHashCode`.
  - Prevented potential NaN from Haversine calculation by clamping intermediate values.
  - Added explicit NaN/Infinity validation for latitude and longitude.
  - Added small fast-path and aligned `ToString` precision with equality tolerance.

### Added
- Unit tests covering hash/equality contract, NaN/Infinity validation, and antipodal-distance sanity check.

## [1.1.0] - 2025-02-06

### Added
- `GeoConstants` class centralizing all geographic constants and magic numbers
- `IHotelSearchService` interface with optimized bounding box + Haversine search
- `HotelSearchService` implementation with two-phase search algorithm
- `GeoCoordinateRules` shared FluentValidation extension methods
- `ErrorResponse` centralized DTO in Api/Models
- `ISystemClock` abstraction for testable time operations
- Postman collection and environment for API testing
- MaxPrice constraint ($100,000,000) on Hotel entity

### Changed
- All validators now use shared `GeoCoordinateRules`
- Parameter naming standardized to origin/destination terminology
- `SearchHotelsQueryHandler` now uses `IHotelSearchService`
- `HotelsController` now invalidates cache on Create/Update/Delete

### Fixed
- Null guards added to `InMemoryHotelRepository.AddAsync()` and `UpdateAsync()`
- Pagination bounds validation

### Technical Details
- Search algorithm: O(n) bounding box filter + O(m) Haversine where m << n
- DRY violations eliminated (geo validation was duplicated 5x)
- All 70 tests passing

## [1.0.0] - 2025-02-05

### Added
- GitHub Actions CI/CD workflow (build, test, publish, docker)
- Multi-stage Dockerfile for containerization
- .dockerignore for optimized builds
- .editorconfig for code style consistency

### Technical Details
- CI runs on push to master/develop and on pull requests
- Docker image uses non-root user for security
- Container health checks configured
- Build artifacts retained for 30 days

## [0.8.0] - 2025-02-05

### Added
- Serilog structured logging with configuration from appsettings.json
- Health check endpoints (/health, /health/live, /health/ready)
- Request logging with correlation IDs
- Bootstrap logging for startup errors

### Technical Details
- Serilog replaces default logging
- Health checks support Kubernetes liveness/readiness probes
- Request logs enriched with host, user agent, and correlation ID

## [0.7.0] - 2025-02-05

### Added
- AuthController with JWT token generation endpoint
- JWT Bearer authentication configuration
- Role-based authorization (AdminOnly policy)
- Swagger UI JWT authentication support
- Authorization attributes on write endpoints (POST, PUT, DELETE)

### Technical Details
- Token expiration configurable in appsettings.json
- Admin role assigned to users with username "admin"
- Read endpoints (GET) allow anonymous access
- Swagger displays lock icons for protected endpoints

## [0.6.0] - 2025-02-05

### Added
- SearchController with output caching support
- ASP.NET Output Cache configuration with custom policies
- Cache varies by latitude, longitude, page, pageSize

### Technical Details
- Search results cached for 2 minutes
- Ready for cache invalidation on hotel changes

## [0.5.0] - 2025-02-05

### Added
- HotelsController with full CRUD operations
- Program.cs with dependency injection setup
- ExceptionMiddleware for global error handling
- CorrelationIdMiddleware for request tracing
- ServiceCollectionExtensions for application services registration
- Swagger/OpenAPI documentation

### Technical Details
- All REST endpoints implemented following HTTP standards
- Proper status codes (201 Created, 204 NoContent, etc.)

## [0.4.0] - 2025-02-05

### Added
- InMemoryHotelRepository with thread-safe ConcurrentDictionary
- PollyPolicies for resilience (retry, circuit breaker, timeout)
- CacheKeys constants for caching
- InfrastructureServiceExtensions for DI registration

### Technical Details
- Repository is singleton to maintain state across requests
- Polly configured with exponential backoff

## [0.3.0] - 2025-02-05

### Added
- CreateHotelCommand with handler and FluentValidation validator
- UpdateHotelCommand with handler and validator
- DeleteHotelCommand with handler
- GetHotelByIdQuery with handler
- GetAllHotelsQuery with handler
- SearchHotelsQuery with handler, validator, and sorting algorithm
- Search algorithm using normalized price/distance with 50/50 weighting
- Pagination support for search results

### Technical Details
- All CQRS commands and queries implemented
- Search sorts hotels by combined score (cheaper + closer = higher rank)

## [0.2.0] - 2025-02-05

### Added
- CQRS foundation with custom IRequest, IRequestHandler, IDispatcher interfaces
- Dispatcher implementation (MediatR-compatible for easy migration)
- IHotelRepository interface for data access abstraction
- DTOs: HotelDto, HotelSearchResultDto, PagedResultDto
- HotelMappingExtensions for entity-to-DTO mapping
- ValidationBehavior for FluentValidation pipeline
- LoggingBehavior for request/response logging

### Technical Details
- CQRS pattern fully implemented without external dependencies
- Ready for MediatR migration if needed

## [0.1.0] - 2025-02-05

### Added
- Initial project structure with Clean Architecture
- Domain layer with Hotel entity and GeoLocation value object
- Solution setup with .NET 10
- Project references and NuGet packages configured
- Domain exceptions (DomainException, HotelNotFoundException)
- Haversine formula implementation for distance calculation
- Unit test and integration test project scaffolding

### Technical Details
- Framework: .NET 10
- Architecture: Clean Architecture with DDD principles
- CQRS pattern (without MediatR) - foundation prepared
