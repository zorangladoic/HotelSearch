using HotelSearch.Domain.Entities;

namespace HotelSearch.Application.Interfaces;

/// <summary>
/// Repository interface for Hotel entity CRUD operations.
/// Follows the Repository pattern - pure data access, no business logic.
/// </summary>
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Hotel> AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task<Hotel> UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
