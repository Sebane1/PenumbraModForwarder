using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Services;

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
        services.AddSingleton<IFileWatcherService, FileWatcherService>();
        services.AddSingleton<ITexToolsHelper, TexToolsHelper>();
        services.AddSingleton<IRegistryHelper, RegistryHelper>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddTransient<IFileWatcher, FileWatcher>();
        services.AddSingleton<IArchiveExtractionService, ArchiveExtractionService>();
        services.AddSingleton<IFileSystemHelper, FileSystemHelper>();
        services.AddSingleton<IModHandlerService, ModHandlerService>();
        services.SetupLogging();
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "BackgroundWorker");
    }
}