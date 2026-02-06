using FluentAssertions;
using HotelSearch.Domain.Common;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.UnitTests.Domain;

public class GeoLocationExtraTests
{
    [Fact]
    public void GetHashCode_WithinEqualityTolerance_ShouldBeEqual()
    {
        // Arrange
        var baseLat = 45.815;
        var baseLon = 15.982;

        // Difference smaller than the EqualityTolerance (1e-7)
        var nearbyLat = baseLat + 0.5e-7; // 5e-8
        var nearbyLon = baseLon - 0.5e-7;

        var loc1 = GeoLocation.Create(baseLat, baseLon);
        var loc2 = GeoLocation.Create(nearbyLat, nearbyLon);

        // Act & Assert
        loc1.Equals(loc2).Should().BeTrue();
        loc1.GetHashCode().Should().Be(loc2.GetHashCode());
    }

    [Fact]
    public void Create_WithNaNOrInfinity_ShouldThrow()
    {
        // Arrange / Act / Assert
        Action actNaN = () => GeoLocation.Create(double.NaN, 0);
        actNaN.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("latitude");

        Action actInf = () => GeoLocation.Create(0, double.PositiveInfinity);
        actInf.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("longitude");
    }

    [Fact]
    public void DistanceTo_AntipodalPoints_ShouldBeApproximatelyHalfCircumference()
    {
        // Arrange - pick a point and its antipode (lat -> -lat, lon -> lon ±180 normalized to [-180,180])
        var p1 = GeoLocation.Create(10.0, 20.0);
        var p2 = GeoLocation.Create(-10.0, -160.0); // antipode of (10, 20)

        // Act
        var distance = p1.DistanceTo(p2);

        // Assert - half Earth's circumference ≈ pi * R
        var expected = Math.PI * GeoConstants.EarthRadiusKm;
        distance.Should().BeApproximately(expected, 50); // 50 km tolerance
    }
}
