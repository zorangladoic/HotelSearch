using FluentAssertions;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.UnitTests.Domain;

public class GeoLocationTests
{
    [Fact]
    public void Create_WithValidCoordinates_ShouldCreateGeoLocation()
    {
        // Arrange
        const double latitude = 45.815;
        const double longitude = 15.982;

        // Act
        var location = GeoLocation.Create(latitude, longitude);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-90, 0)]
    [InlineData(90, 0)]
    [InlineData(0, -180)]
    [InlineData(0, 180)]
    [InlineData(0, 0)]
    public void Create_WithBoundaryCoordinates_ShouldSucceed(double latitude, double longitude)
    {
        // Act
        var location = GeoLocation.Create(latitude, longitude);

        // Assert
        location.Should().NotBeNull();
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(-90.001, 0)]
    [InlineData(90.001, 0)]
    public void Create_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => GeoLocation.Create(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [InlineData(0, -180.001)]
    [InlineData(0, 180.001)]
    public void Create_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        var act = () => GeoLocation.Create(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void DistanceTo_SameLocation_ShouldReturnZero()
    {
        // Arrange
        var location1 = GeoLocation.Create(45.815, 15.982);
        var location2 = GeoLocation.Create(45.815, 15.982);

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceTo_KnownLocations_ShouldReturnCorrectDistance()
    {
        // Arrange - Zagreb to Vienna (approximately 300 km)
        var zagreb = GeoLocation.Create(45.815, 15.982);
        var vienna = GeoLocation.Create(48.208, 16.373);

        // Act
        var distance = zagreb.DistanceTo(vienna);

        // Assert - Allow some tolerance for the Haversine approximation
        distance.Should().BeApproximately(268, 15); // ~268 km with 15 km tolerance
    }

    [Fact]
    public void DistanceTo_NullLocation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var location = GeoLocation.Create(45.815, 15.982);

        // Act
        var act = () => location.DistanceTo(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equals_SameCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var location1 = GeoLocation.Create(45.815, 15.982);
        var location2 = GeoLocation.Create(45.815, 15.982);

        // Act & Assert
        location1.Equals(location2).Should().BeTrue();
        (location1 == location2).Should().BeTrue();
        (location1 != location2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentCoordinates_ShouldReturnFalse()
    {
        // Arrange
        var location1 = GeoLocation.Create(45.815, 15.982);
        var location2 = GeoLocation.Create(48.208, 16.373);

        // Act & Assert
        location1.Equals(location2).Should().BeFalse();
        (location1 == location2).Should().BeFalse();
        (location1 != location2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        // Arrange
        var location = GeoLocation.Create(45.815, 15.982);

        // Act & Assert
        location.Equals(null).Should().BeFalse();
        (location == null).Should().BeFalse();
        (null == location).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameCoordinates_ShouldReturnSameHash()
    {
        // Arrange
        var location1 = GeoLocation.Create(45.815, 15.982);
        var location2 = GeoLocation.Create(45.815, 15.982);

        // Act & Assert
        location1.GetHashCode().Should().Be(location2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var location = GeoLocation.Create(45.815, 15.982);

        // Act
        var result = location.ToString();

        // Assert - Check that coordinates are in the string (format may vary by locale)
        result.Should().Contain("45");
        result.Should().Contain("15");
        result.Should().StartWith("(");
        result.Should().EndWith(")");
    }
}
