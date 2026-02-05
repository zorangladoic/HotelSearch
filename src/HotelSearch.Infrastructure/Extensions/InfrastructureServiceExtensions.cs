using HotelSearch.Application.Interfaces;
using HotelSearch.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSearch.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IHotelRepository, InMemoryHotelRepository>();

        return services;
    }
}
