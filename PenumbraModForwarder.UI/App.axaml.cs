using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI;

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
            bool isInitialized = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";
            _logger.Debug("Application initialized by watchdog: {IsInitialized}", isInitialized);

            var args = Environment.GetCommandLineArgs();
            _logger.Debug("Command-line arguments: {Args}", string.Join(", ", args));

            if (!isInitialized)
            {
                _logger.Warn("Application not initialized by watchdog, showing error window");
                desktop.MainWindow = new ErrorWindow
                {
                    DataContext = ActivatorUtilities.CreateInstance<ErrorWindowViewModel>(_serviceProvider)
                };
            }
            else
            {
                _logger.Debug("Showing main window");

                if (args.Length < 2)
                {
                    _logger.Fatal("No port specified for the UI.");
                    Environment.Exit(1);
                }

                int port = int.Parse(args[1]);
                _logger.Info("Listening on port {Port}", port);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, port)
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}