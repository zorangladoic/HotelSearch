namespace HotelSearch.Application.Common.Interfaces;

/// <summary>
/// Marker interface for CQRS requests (commands and queries).
/// Compatible with MediatR's IRequest for easy migration.
/// </summary>
public interface IRequest<TResponse> { }
