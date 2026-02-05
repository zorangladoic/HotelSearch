using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelSearch.IntegrationTests.Controllers;

public class HotelsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HotelsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/hotels");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/hotels/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var hotel = new
        {
            Name = "Test Hotel",
            PricePerNight = 100.00m,
            Latitude = 45.815,
            Longitude = 15.982
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/hotels", hotel);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithAuth_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var hotel = new
        {
            Name = "Test Hotel",
            PricePerNight = 100.00m,
            Latitude = 45.815,
            Longitude = 15.982
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/hotels", hotel);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var createdHotel = JsonSerializer.Deserialize<HotelResponse>(content, _jsonOptions);

        createdHotel.Should().NotBeNull();
        createdHotel!.Name.Should().Be("Test Hotel");
        createdHotel.PricePerNight.Should().Be(100.00m);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        var hotel = new
        {
            Name = "", // Invalid - empty name
            PricePerNight = 100.00m,
            Latitude = 45.815,
            Longitude = 15.982
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/hotels", hotel);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrudOperations_ShouldWorkCorrectly()
    {
        // Arrange
        await AuthenticateAsAdminAsync();

        // Create
        var createRequest = new
        {
            Name = "CRUD Test Hotel",
            PricePerNight = 150.00m,
            Latitude = 45.815,
            Longitude = 15.982
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/hotels", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdHotel = JsonSerializer.Deserialize<HotelResponse>(createContent, _jsonOptions);
        var hotelId = createdHotel!.Id;

        // Read
        var getResponse = await _client.GetAsync($"/api/v1/hotels/{hotelId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var fetchedHotel = JsonSerializer.Deserialize<HotelResponse>(getContent, _jsonOptions);
        fetchedHotel!.Name.Should().Be("CRUD Test Hotel");

        // Update
        var updateRequest = new
        {
            Name = "Updated CRUD Test Hotel",
            PricePerNight = 200.00m,
            Latitude = 46.0,
            Longitude = 16.0
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/hotels/{hotelId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedHotel = JsonSerializer.Deserialize<HotelResponse>(updateContent, _jsonOptions);
        updatedHotel!.Name.Should().Be("Updated CRUD Test Hotel");
        updatedHotel.PricePerNight.Should().Be(200.00m);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/v1/hotels/{hotelId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/v1/hotels/{hotelId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/hotels/{Guid.NewGuid()}", new
        {
            Name = "Test",
            PricePerNight = 100m,
            Latitude = 45.0,
            Longitude = 15.0
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/v1/hotels/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    private record HotelResponse(
        Guid Id,
        string Name,
        decimal PricePerNight,
        double Latitude,
        double Longitude);

    private record TokenResponse(string AccessToken, int ExpiresInMinutes);
}
