using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;

namespace PenumbraModForwarder.BackgroundWorker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, int port)
    {
        services.AddHostedService(provider => new Worker(
            provider.GetRequiredService<IWebSocketServer>(),
            provider.GetRequiredService<IStartupService>(),
            port
        ));
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<ITexToolsHelper, TexToolsHelper>();
        services.AddSingleton<IRegistryHelper, RegistryHelper>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.SetupLogging();
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "BackgroundWorker");
    }
}