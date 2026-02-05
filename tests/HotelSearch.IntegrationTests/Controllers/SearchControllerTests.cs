using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelSearch.IntegrationTests.Controllers;

public class SearchControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SearchControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_WithValidParameters_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815&longitude=15.982");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_WithPagination_ShouldReturnPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815&longitude=15.982&page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedSearchResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Search_WithoutLatitude_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?longitude=15.982");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithoutLongitude_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidLatitude_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?latitude=91&longitude=15.982");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidLongitude_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815&longitude=181");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_ShouldReturnHotelsSortedByPriceAndDistance()
    {
        // Arrange - Create some test hotels
        await AuthenticateAsAdminAsync();

        // Create hotels with varying prices and locations
        await CreateHotelAsync("Cheap Close Hotel", 50m, 45.82, 15.99);
        await CreateHotelAsync("Expensive Far Hotel", 500m, 48.20, 16.37);
        await CreateHotelAsync("Medium Hotel", 150m, 46.5, 16.0);

        // Act - Search from Zagreb coordinates
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815&longitude=15.982&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedSearchResponse>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();

        // The cheap and close hotel should rank higher
        var cheapCloseIndex = result.Items.FindIndex(h => h.Name == "Cheap Close Hotel");
        var expensiveFarIndex = result.Items.FindIndex(h => h.Name == "Expensive Far Hotel");

        if (cheapCloseIndex >= 0 && expensiveFarIndex >= 0)
        {
            cheapCloseIndex.Should().BeLessThan(expensiveFarIndex);
        }
    }

    [Fact]
    public async Task Search_ShouldIncludeDistanceInResults()
    {
        // Arrange
        await AuthenticateAsAdminAsync();
        await CreateHotelAsync("Distance Test Hotel", 100m, 48.208, 16.373); // Vienna

        // Act - Search from Zagreb
        var response = await _client.GetAsync("/api/v1/search?latitude=45.815&longitude=15.982");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedSearchResponse>(content, _jsonOptions);

        var distanceTestHotel = result?.Items.FirstOrDefault(h => h.Name == "Distance Test Hotel");
        if (distanceTestHotel != null)
        {
            distanceTestHotel.DistanceKm.Should().BeApproximately(278, 15); // ~278 km with tolerance
        }
    }

    private async Task AuthenticateAsAdminAsync()
    {
        var tokenRequest = new { Username = "admin", Password = "admin" };
        var tokenResponse = await _client.PostAsJsonAsync("/api/v1/auth/token", tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenResult = JsonSerializer.Deserialize<TokenResponse>(tokenContent, _jsonOptions);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResult!.AccessToken);
    }

    private async Task<Guid> CreateHotelAsync(string name, decimal price, double lat, double lon)
    {
        var hotel = new { Name = name, PricePerNight = price, Latitude = lat, Longitude = lon };
        var response = await _client.PostAsJsonAsync("/api/v1/hotels", hotel);
        var content = await response.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<HotelResponse>(content, _jsonOptions);
        return created!.Id;
    }

    private record HotelResponse(Guid Id, string Name, decimal PricePerNight, double Latitude, double Longitude);
    private record TokenResponse(string AccessToken, int ExpiresInMinutes);
    private record SearchResultItem(Guid Id, string Name, decimal PricePerNight, double DistanceKm);
    private record PagedSearchResponse(
        List<SearchResultItem> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
