using habit_service.Interface;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace habit_service.Services;

public static class ServiceDependancies
{
    public static IServiceCollection ServiceInjector(this IServiceCollection service)
    {
        service.AddScoped<IHabitService, HabitService>();
        return service;
    }
}
