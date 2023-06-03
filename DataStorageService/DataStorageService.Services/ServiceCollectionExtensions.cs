using DataStorageService.Services.Services;
using DataStorageService.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataStorageService.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddScoped<ICsvDataService, CsvDataService>();
}