using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.Statistics.Services;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Setup logging and other services
        services.SetupLogging();

        // Register ConfigurationModel as a singleton
        services.AddSingleton<ConfigurationModel>();

        // Services
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IWebSocketClient, WebSocketClient>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IXivLauncherService, XivLauncherService>();
        services.AddSingleton<IConfigurationListener, ConfigurationListener>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IStatisticService, StatisticService>();
        services.AddSingleton<IFileDialogService>(provider =>
        {
            var applicationLifetime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var mainWindow = applicationLifetime?.MainWindow;

            if (mainWindow == null)
            {
                throw new InvalidOperationException("MainWindow is not initialized.");
            }

            return new FileDialogService(mainWindow);
        });
        services.AddSingleton<IXmaModDisplay, XmaModDisplay>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<ErrorWindowViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ModsViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DownloadViewModel>();
        
        // Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ErrorWindowViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<ModsViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<DownloadViewModel>();
        
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "UI");
    }
}