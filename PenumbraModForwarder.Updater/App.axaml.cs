using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.Updater.ViewModels;
using PenumbraModForwarder.Updater.Views;

namespace PenumbraModForwarder.Updater;

public partial class App : Application
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        try
        {
            _serviceProvider = Program.ServiceProvider;
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Failed to initialize ServiceProvider");
            Environment.Exit(1);
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}