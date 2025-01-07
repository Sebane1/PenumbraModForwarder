using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Updater.ViewModels;
using PenumbraModForwarder.Updater.Views;
using Serilog;

namespace PenumbraModForwarder.Updater;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public App()
    {
        _logger = Log.ForContext<App>();
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}