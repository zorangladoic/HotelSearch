using System.Collections.Concurrent;
using HotelSearch.Application.Interfaces;
using HotelSearch.Domain.Entities;

namespace HotelSearch.Infrastructure.Repositories;

public sealed class InMemoryHotelRepository : IHotelRepository
{
    private readonly ConcurrentDictionary<Guid, Hotel> _hotels = new();

    public Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _hotels.TryGetValue(id, out var hotel);
        return Task.FromResult(hotel);
    }

    public Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var hotels = _hotels.Values.ToList();
        return Task.FromResult<IReadOnlyList<Hotel>>(hotels);
    }

    public Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hotel);

        if (!_hotels.TryAdd(hotel.Id, hotel))
        {
            throw new InvalidOperationException($"Hotel with ID {hotel.Id} already exists.");
        }

        return Task.FromResult(hotel);
    }

    public Task<Hotel> UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hotel);

        if (!_hotels.ContainsKey(hotel.Id))
        {
            throw new InvalidOperationException($"Hotel with ID {hotel.Id} does not exist.");
        }

        _hotels[hotel.Id] = hotel;
        return Task.FromResult(hotel);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _hotels.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = _hotels.ContainsKey(id);
        return Task.FromResult(exists);
    }
}
