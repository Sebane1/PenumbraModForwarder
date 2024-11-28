using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Watchdog.Interfaces;
using PenumbraModForwarder.Watchdog.Services;

namespace PenumbraModForwarder.Watchdog.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<IConfigurationService, ConfigurationService>();
        services.AddTransient<IConfigurationSetup, ConfigurationSetup>();
        
        
        services.SetupLogging();
        
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "WatchDog");
    }
}