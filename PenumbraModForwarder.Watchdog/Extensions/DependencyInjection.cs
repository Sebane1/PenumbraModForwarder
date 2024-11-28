using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;

namespace PenumbraModForwarder.Watchdog.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IConfigurationService, ConfigurationService>();
        
        
        services.SetupLogging();
        
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "WatchDog");
    }
}