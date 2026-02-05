using FluentAssertions;
using HotelSearch.Domain.Entities;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.UnitTests.Domain;

public class HotelTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateHotel()
    {
        // Arrange
        const string name = "Grand Hotel";
        const decimal price = 150.00m;
        const double latitude = 45.815;
        const double longitude = 15.982;

        // Act
        var hotel = Hotel.Create(name, price, latitude, longitude);

        // Assert
        hotel.Should().NotBeNull();
        hotel.Id.Should().NotBeEmpty();
        hotel.Name.Should().Be(name);
        hotel.PricePerNight.Should().Be(price);
        hotel.Location.Latitude.Should().Be(latitude);
        hotel.Location.Longitude.Should().Be(longitude);
        hotel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        hotel.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNameHavingWhitespace_ShouldTrimName()
    {
        // Arrange
        const string nameWithSpaces = "  Grand Hotel  ";

        // Act
        var hotel = Hotel.Create(nameWithSpaces, 100m, 45.0, 15.0);

        // Assert
        hotel.Name.Should().Be("Grand Hotel");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrNullName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Hotel.Create(name!, 100m, 45.0, 15.0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithNameExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('A', Hotel.MaxNameLength + 1);

        // Act
        var act = () => Hotel.Create(longName, 100m, 45.0, 15.0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name")
            .WithMessage($"*cannot exceed {Hotel.MaxNameLength} characters*");
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var maxLengthName = new string('A', Hotel.MaxNameLength);

        // Act
        var hotel = Hotel.Create(maxLengthName, 100m, 45.0, 15.0);

        // Assert
        hotel.Name.Should().HaveLength(Hotel.MaxNameLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidPrice_ShouldThrowArgumentOutOfRangeException(decimal price)
    {
        // Act
        var act = () => Hotel.Create("Hotel", price, 45.0, 15.0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pricePerNight");
    }

    [Fact]
    public void Create_WithMinimumPrice_ShouldSucceed()
    {
        // Act
        var hotel = Hotel.Create("Hotel", Hotel.MinPrice, 45.0, 15.0);

        // Assert
        hotel.PricePerNight.Should().Be(Hotel.MinPrice);
    }

    [Fact]
    public void Create_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => Hotel.Create("Hotel", 100m, 91, 15.0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Fact]
    public void Create_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        var act = () => Hotel.Create("Hotel", 100m, 45.0, 181);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateHotel()
    {
        // Arrange
        var hotel = Hotel.Create("Original Hotel", 100m, 45.0, 15.0);
        var originalCreatedAt = hotel.CreatedAt;

        // Act
        hotel.Update("Updated Hotel", 200m, 48.0, 16.0);

        // Assert
        hotel.Name.Should().Be("Updated Hotel");
        hotel.PricePerNight.Should().Be(200m);
        hotel.Location.Latitude.Should().Be(48.0);
        hotel.Location.Longitude.Should().Be(16.0);
        hotel.CreatedAt.Should().Be(originalCreatedAt);
        hotel.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithInvalidName_ShouldThrowArgumentException()
    {
        // Arrange
        var hotel = Hotel.Create("Hotel", 100m, 45.0, 15.0);

        // Act
        var act = () => hotel.Update("", 100m, 45.0, 15.0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithInvalidPrice_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var hotel = Hotel.Create("Hotel", 100m, 45.0, 15.0);

        // Act
        var act = () => hotel.Update("Hotel", 0, 45.0, 15.0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DistanceTo_ShouldCalculateDistance()
    {
        // Arrange
        var hotel = Hotel.Create("Hotel", 100m, 45.815, 15.982);
        var userLocation = GeoLocation.Create(48.208, 16.373);

        // Act
        var distance = hotel.DistanceTo(userLocation);

        // Assert
        distance.Should().BeApproximately(268, 15);
    }

    [Fact]
    public void CreateWithId_ShouldCreateHotelWithSpecificId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var hotel = Hotel.CreateWithId(id, "Hotel", 100m, 45.0, 15.0, createdAt, updatedAt);

        // Assert
        hotel.Id.Should().Be(id);
        hotel.CreatedAt.Should().Be(createdAt);
        hotel.UpdatedAt.Should().Be(updatedAt);
    }
}
