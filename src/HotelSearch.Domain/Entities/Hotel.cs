using HotelSearch.Domain.Common;
using HotelSearch.Domain.ValueObjects;

namespace HotelSearch.Domain.Entities;

public sealed class Hotel : Entity
{
    public const int MaxNameLength = 200;
    public const decimal MinPrice = 0.01m;

    public string Name { get; private set; } = string.Empty;
    public decimal PricePerNight { get; private set; }
    public GeoLocation Location { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Hotel() : base() { }

    private Hotel(Guid id, string name, decimal pricePerNight, GeoLocation location) : base(id)
    {
        Name = name;
        PricePerNight = pricePerNight;
        Location = location;
        CreatedAt = DateTime.UtcNow;
    }

    public static Hotel Create(string name, decimal pricePerNight, double latitude, double longitude)
    {
        ValidateName(name);
        ValidatePrice(pricePerNight);

        var location = GeoLocation.Create(latitude, longitude);
        return new Hotel(Guid.NewGuid(), name.Trim(), pricePerNight, location);
    }

    public static Hotel CreateWithId(Guid id, string name, decimal pricePerNight, double latitude, double longitude, DateTime createdAt, DateTime? updatedAt = null)
    {
        ValidateName(name);
        ValidatePrice(pricePerNight);

        var location = GeoLocation.Create(latitude, longitude);
        return new Hotel(id, name.Trim(), pricePerNight, location)
        {
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Update(string name, decimal pricePerNight, double latitude, double longitude)
    {
        ValidateName(name);
        ValidatePrice(pricePerNight);

        Name = name.Trim();
        PricePerNight = pricePerNight;
        Location = GeoLocation.Create(latitude, longitude);
        UpdatedAt = DateTime.UtcNow;
    }

    public double DistanceTo(GeoLocation location) => Location.DistanceTo(location);

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty.", nameof(name));

        if (name.Trim().Length > MaxNameLength)
            throw new ArgumentException($"Hotel name cannot exceed {MaxNameLength} characters.", nameof(name));
    }

    private static void ValidatePrice(decimal pricePerNight)
    {
        if (pricePerNight < MinPrice)
            throw new ArgumentOutOfRangeException(nameof(pricePerNight), pricePerNight, $"Price per night must be at least {MinPrice}.");
    }
}
