using HotelSearch.Application.Interfaces;
using HotelSearch.Infrastructure.Repositories;
using HotelSearch.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repository for data access (singleton to maintain in-memory state)
        services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();

        // Search service for business logic (singleton - stateless)
        services.AddSingleton<IHotelSearchService, HotelSearchService>();

        return services;
    }
}
