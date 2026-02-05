namespace HotelSearch.Infrastructure.Caching;

public static class CacheKeys
{
    public const string AllHotels = "hotels:all";
    public const string HotelPrefix = "hotels:";
    public const string SearchPrefix = "search:";

    public static string Hotel(Guid id) => $"{HotelPrefix}{id}";

    public static string Search(double latitude, double longitude, int page, int pageSize)
        => $"{SearchPrefix}{latitude:F4}:{longitude:F4}:{page}:{pageSize}";
}
