using FluentValidation;
using HotelSearch.Application.Common;
using HotelSearch.Application.Common.Interfaces;
using HotelSearch.Application.Hotels.Commands.CreateHotel;

namespace HotelSearch.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        var applicationAssembly = typeof(CreateHotelCommand).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly);

        var handlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddScoped(handlerInterface, handlerType);
        }

        return services;
    }
}
