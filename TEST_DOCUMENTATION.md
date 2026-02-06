# Test Documentation

This document provides a comprehensive overview of all test cases in the HotelSearch API solution.

## Test Summary

| Category | Test Class | Test Count |
|----------|------------|------------|
| **Unit Tests - Domain** | HotelTests | 18 |
| **Unit Tests - Domain** | GeoLocationTests | 22 |
| **Unit Tests - Domain** | GeoLocationExtraTests | 3 |
| **Unit Tests - Application** | CreateHotelCommandHandlerTests | 3 |
| **Unit Tests - Application** | SearchHotelsQueryHandlerTests | 8 |
| **Integration Tests** | HotelsControllerTests | 8 |
| **Integration Tests** | SearchControllerTests | 8 |
| **Integration Tests** | HealthCheckTests | 3 |
| **Total** | | **73** |

> **Note:** Theory tests with multiple `[InlineData]` attributes count as separate test cases. For example, `Create_WithBoundaryCoordinates_ShouldSucceed` has 5 data rows, so it counts as 5 tests.

---

## Unit Tests

### Domain Layer

#### HotelTests

**File:** `tests/HotelSearch.UnitTests/Domain/HotelTests.cs`

Tests for the `Hotel` entity class covering creation, update, and validation logic. **18 tests total** (includes Theory test cases).

| # | Test Name | Description | Type |
|---|-----------|-------------|------|
| 1 | `Create_WithValidData_ShouldCreateHotel` | Verifies hotel creation with valid name, price, and coordinates. Checks all properties including auto-generated Id and CreatedAt timestamp. | Fact |
| 2 | `Create_WithNameHavingWhitespace_ShouldTrimName` | Ensures hotel names are trimmed of leading/trailing whitespace. | Fact |
| 3 | `Create_WithEmptyOrNullName_ShouldThrowArgumentException` | Tests that null, empty, or whitespace-only names throw ArgumentException. | Theory |
| | | - `null` | |
| | | - `""` (empty string) | |
| | | - `"   "` (whitespace only) | |
| 4 | `Create_WithNameExceedingMaxLength_ShouldThrowArgumentException` | Validates name length constraint (max 200 characters). | Fact |
| 5 | `Create_WithNameAtMaxLength_ShouldSucceed` | Confirms names at exactly max length are accepted. | Fact |
| 6 | `Create_WithInvalidPrice_ShouldThrowArgumentOutOfRangeException` | Tests price validation rejects zero and negative values. | Theory |
| | | - `0` | |
| | | - `-1` | |
| | | - `-100` | |
| 7 | `Create_WithMinimumPrice_ShouldSucceed` | Confirms minimum valid price (0.01) is accepted. | Fact |
| 8 | `Create_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException` | Tests latitude validation (must be between -90 and 90). | Fact |
| 9 | `Create_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException` | Tests longitude validation (must be between -180 and 180). | Fact |
| 10 | `Update_WithValidData_ShouldUpdateHotel` | Verifies hotel update modifies all properties and sets UpdatedAt timestamp while preserving CreatedAt. | Fact |
| 11 | `Update_WithInvalidName_ShouldThrowArgumentException` | Tests update validation rejects empty names. | Fact |
| 12 | `Update_WithInvalidPrice_ShouldThrowArgumentOutOfRangeException` | Tests update validation rejects zero price. | Fact |
| 13 | `DistanceTo_ShouldCalculateDistance` | Verifies Haversine distance calculation from hotel to a user location. | Fact |
| 14 | `CreateWithId_ShouldCreateHotelWithSpecificId` | Tests factory method for creating hotel with pre-defined ID and timestamps (useful for hydration from storage). | Fact |

---

#### GeoLocationTests

**File:** `tests/HotelSearch.UnitTests/Domain/GeoLocationTests.cs`

Tests for the `GeoLocation` value object covering coordinate validation, distance calculation, and equality. **22 tests total** (includes Theory test cases).

| # | Test Name | Description | Type |
|---|-----------|-------------|------|
| 1 | `Create_WithValidCoordinates_ShouldCreateGeoLocation` | Verifies basic creation with valid latitude/longitude. | Fact |
| 2 | `Create_WithBoundaryCoordinates_ShouldSucceed` | Tests all boundary values are accepted. | Theory |
| | | - `(-90, 0)` - South Pole | |
| | | - `(90, 0)` - North Pole | |
| | | - `(0, -180)` - Antimeridian West | |
| | | - `(0, 180)` - Antimeridian East | |
| | | - `(0, 0)` - Null Island | |
| 3 | `Create_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException` | Tests invalid latitude values are rejected. | Theory |
| | | - `(-91, 0)` | |
| | | - `(91, 0)` | |
| | | - `(-90.001, 0)` | |
| | | - `(90.001, 0)` | |
| 4 | `Create_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException` | Tests invalid longitude values are rejected. | Theory |
| | | - `(0, -181)` | |
| | | - `(0, 181)` | |
| | | - `(0, -180.001)` | |
| | | - `(0, 180.001)` | |
| 5 | `DistanceTo_SameLocation_ShouldReturnZero` | Verifies distance to same point is zero. | Fact |
| 6 | `DistanceTo_KnownLocations_ShouldReturnCorrectDistance` | Tests Haversine formula accuracy using Zagreb to Vienna (~268 km). | Fact |
| 7 | `DistanceTo_NullLocation_ShouldThrowArgumentNullException` | Tests null guard on distance calculation. | Fact |
| 8 | `Equals_SameCoordinates_ShouldReturnTrue` | Verifies equality for identical coordinates. | Fact |
| 9 | `Equals_DifferentCoordinates_ShouldReturnFalse` | Verifies inequality for different coordinates. | Fact |
| 10 | `Equals_Null_ShouldReturnFalse` | Tests null comparison returns false. | Fact |
| 11 | `GetHashCode_SameCoordinates_ShouldReturnSameHash` | Verifies hash code consistency for equal objects. | Fact |
| 12 | `ToString_ShouldReturnFormattedString` | Validates string representation format "(lat, lon)". | Fact |

---

#### GeoLocationExtraTests

**File:** `tests/HotelSearch.UnitTests/Domain/GeoLocationExtraTests.cs`

Additional edge case tests for `GeoLocation` value object. **3 tests total.**

| # | Test Name | Description | Type |
|---|-----------|-------------|------|
| 1 | `GetHashCode_WithinEqualityTolerance_ShouldBeEqual` | Verifies hash code consistency for coordinates within equality tolerance (1e-7). Ensures hash/equality contract is maintained. | Fact |
| 2 | `Create_WithNaNOrInfinity_ShouldThrow` | Tests that NaN and Infinity values are rejected for both latitude and longitude. | Fact |
| 3 | `DistanceTo_AntipodalPoints_ShouldBeApproximatelyHalfCircumference` | Tests edge case of antipodal points (opposite sides of Earth). Distance should be approximately π × R (~20,015 km). | Fact |

---

### Application Layer

#### CreateHotelCommandHandlerTests

**File:** `tests/HotelSearch.UnitTests/Application/Commands/CreateHotelCommandHandlerTests.cs`

Tests for the `CreateHotelCommandHandler` CQRS command handler. **3 tests total.**

| # | Test Name | Description | Type |
|---|-----------|-------------|------|
| 1 | `Handle_WithValidCommand_ShouldCreateHotelAndReturnDto` | Verifies successful hotel creation returns correct DTO with all properties populated. | Fact |
| 2 | `Handle_ShouldTrimHotelName` | Ensures name trimming is applied during creation. | Fact |
| 3 | `Handle_WithRepositoryException_ShouldPropagateException` | Tests that repository exceptions bubble up correctly. | Fact |

---

#### SearchHotelsQueryHandlerTests

**File:** `tests/HotelSearch.UnitTests/Application/Queries/SearchHotelsQueryHandlerTests.cs`

Tests for the `SearchHotelsQueryHandler` covering search logic, sorting algorithm, and pagination. **8 tests total.**

| # | Test Name | Description | Type |
|---|-----------|-------------|------|
| 1 | `Handle_WithNoHotels_ShouldReturnEmptyResult` | Verifies empty repository returns proper empty paged result. | Fact |
| 2 | `Handle_WithHotels_ShouldReturnSortedByPriceAndDistance` | Tests combined price/distance sorting algorithm. Cheap & close hotels should rank higher than expensive & far hotels. | Fact |
| 3 | `Handle_WithPagination_ShouldReturnCorrectPage` | Verifies pagination returns correct page (page 2 of 25 items with pageSize 10). | Fact |
| 4 | `Handle_WithLastPage_ShouldReturnRemainingItems` | Tests last page returns only remaining items (5 items on page 3 of 25). | Fact |
| 5 | `Handle_WithSingleHotel_ShouldReturnSingleResult` | Tests edge case with single hotel in repository. | Fact |
| 6 | `Handle_ShouldIncludeDistanceInResults` | Verifies distance calculation is included in search results (Zagreb to Vienna ~268 km). | Fact |
| 7 | `Handle_WithSamePriceHotels_ShouldSortByDistance` | Tests that when prices are equal, closer hotels rank higher. | Fact |
| 8 | `Handle_WithSameDistanceHotels_ShouldSortByPrice` | Tests that when distances are equal, cheaper hotels rank higher. | Fact |

---

## Integration Tests

### HotelsControllerTests

**File:** `tests/HotelSearch.IntegrationTests/Controllers/HotelsControllerTests.cs`

End-to-end tests for the Hotels CRUD API endpoints. **8 tests total.**

| # | Test Name | Description | HTTP Method | Endpoint |
|---|-----------|-------------|-------------|----------|
| 1 | `GetAll_ShouldReturnOk` | Verifies GET all hotels returns 200 OK. | GET | `/api/v1/hotels` |
| 2 | `GetById_WithNonExistentId_ShouldReturnNotFound` | Tests 404 response for non-existent hotel ID. | GET | `/api/v1/hotels/{id}` |
| 3 | `Create_WithoutAuth_ShouldReturnUnauthorized` | Verifies authentication required for hotel creation. | POST | `/api/v1/hotels` |
| 4 | `Create_WithAuth_ShouldReturnCreated` | Tests successful hotel creation with admin token returns 201 Created. | POST | `/api/v1/hotels` |
| 5 | `Create_WithInvalidData_ShouldReturnBadRequest` | Tests validation error (empty name) returns 400 Bad Request. | POST | `/api/v1/hotels` |
| 6 | `CrudOperations_ShouldWorkCorrectly` | Full CRUD lifecycle test: Create → Read → Update → Delete → Verify deletion. | All | `/api/v1/hotels` |
| 7 | `Update_WithoutAuth_ShouldReturnUnauthorized` | Verifies authentication required for hotel update. | PUT | `/api/v1/hotels/{id}` |
| 8 | `Delete_WithoutAuth_ShouldReturnUnauthorized` | Verifies authentication required for hotel deletion. | DELETE | `/api/v1/hotels/{id}` |

**Authentication:** Tests use JWT token obtained from `/api/v1/auth/token` with admin credentials.

---

### SearchControllerTests

**File:** `tests/HotelSearch.IntegrationTests/Controllers/SearchControllerTests.cs`

End-to-end tests for the Search API endpoint. **8 tests total.**

| # | Test Name | Description | HTTP Method | Endpoint |
|---|-----------|-------------|-------------|----------|
| 1 | `Search_WithValidParameters_ShouldReturnOk` | Verifies basic search with valid coordinates returns 200 OK. | GET | `/api/v1/search` |
| 2 | `Search_WithPagination_ShouldReturnPagedResults` | Tests pagination parameters are respected in response. | GET | `/api/v1/search` |
| 3 | `Search_WithoutLatitude_ShouldReturnBadRequest` | Tests latitude is required. | GET | `/api/v1/search` |
| 4 | `Search_WithoutLongitude_ShouldReturnBadRequest` | Tests longitude is required. | GET | `/api/v1/search` |
| 5 | `Search_WithInvalidLatitude_ShouldReturnBadRequest` | Tests latitude validation (91 is invalid). | GET | `/api/v1/search` |
| 6 | `Search_WithInvalidLongitude_ShouldReturnBadRequest` | Tests longitude validation (181 is invalid). | GET | `/api/v1/search` |
| 7 | `Search_ShouldReturnHotelsSortedByPriceAndDistance` | Creates test hotels and verifies sorting algorithm (cheap & close ranks higher than expensive & far). | GET | `/api/v1/search` |
| 8 | `Search_ShouldIncludeDistanceInResults` | Verifies distance calculation is included in search response. | GET | `/api/v1/search` |

---

### HealthCheckTests

**File:** `tests/HotelSearch.IntegrationTests/Controllers/HealthCheckTests.cs`

Tests for application health check endpoints (Kubernetes liveness/readiness probes). **3 tests total.**

| # | Test Name | Description | HTTP Method | Endpoint |
|---|-----------|-------------|-------------|----------|
| 1 | `Health_ShouldReturnHealthy` | Verifies main health endpoint returns "Healthy" status with all checks. | GET | `/health` |
| 2 | `HealthLive_ShouldReturnHealthy` | Tests Kubernetes liveness probe endpoint. | GET | `/health/live` |
| 3 | `HealthReady_ShouldReturnHealthy` | Tests Kubernetes readiness probe endpoint. | GET | `/health/ready` |

---

## Test Coverage by Feature

### Hotel Entity
- Creation with valid/invalid data
- Name validation (empty, whitespace, max length, trimming)
- Price validation (zero, negative, minimum)
- Coordinate validation (latitude/longitude bounds)
- Update operations
- Distance calculation

### GeoLocation Value Object
- Coordinate validation (bounds, boundary values, NaN/Infinity)
- Haversine distance formula (same location, known distances, antipodal points)
- Equality/hash code contract
- Tolerance-based equality

### Search Algorithm
- Sorting by combined price/distance score
- Pagination (page size, total pages, last page)
- Distance calculation in results
- Edge cases (empty results, single hotel)

### CRUD Operations
- Create (with/without auth, valid/invalid data)
- Read (by ID, all hotels, not found)
- Update (with/without auth)
- Delete (with/without auth)
- Full lifecycle test

### API Validation
- Required parameters
- Coordinate range validation
- Authentication requirements

### Health Checks
- Main health endpoint
- Liveness probe
- Readiness probe

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test tests/HotelSearch.UnitTests
```

### Run Integration Tests Only
```bash
dotnet test tests/HotelSearch.IntegrationTests
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~HotelTests"
```

---

## Test Frameworks & Libraries

| Library | Purpose |
|---------|---------|
| **xUnit** | Test framework |
| **FluentAssertions** | Fluent assertion library |
| **Moq** | Mocking framework (unit tests) |
| **Microsoft.AspNetCore.Mvc.Testing** | Integration test host |

---

## Notes

1. **Integration tests** use `WebApplicationFactory<Program>` to spin up an in-memory test server.
2. **Unit tests** use Moq to mock the repository layer.
3. **Authentication tests** obtain JWT tokens via the `/api/v1/auth/token` endpoint.
4. **Distance calculations** use the Haversine formula with ~15 km tolerance for assertions.
5. **GeoLocation equality** uses a tolerance of 1e-7 for coordinate comparison.
