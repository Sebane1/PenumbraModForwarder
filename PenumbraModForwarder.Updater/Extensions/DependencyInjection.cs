using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Updater.Interfaces;
using PenumbraModForwarder.Updater.Services;
using PenumbraModForwarder.Updater.ViewModels;
using PenumbraModForwarder.Updater.Views;

namespace PenumbraModForwarder.Updater.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.SetupLogging();

        services.AddHttpClient<IStaticResourceService, StaticResourceService>();
        services.AddSingleton<IGetBackgroundInformation, GetBackgroundInformation>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IAria2Service>(_ =>
        {
            var aria2InstallFolder = Path.Combine(AppContext.BaseDirectory, "aria2");
            return new Aria2Service(aria2InstallFolder);
        });
        services.AddSingleton<IDownloadAndInstallUpdates, DownloadAndInstallUpdates>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
        
        return services;
    }
    
    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "Updater");
    }
}