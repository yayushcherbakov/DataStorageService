using DataStorageService.Services.Services;
using DataStorageService.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataStorageService.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Simplifies registration and configuration of services in the dependency injection container.
    /// </summary>
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<ICsvDataService, CsvDataService>();
}