using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using Serilog;

namespace PenumbraModForwarder.UI
{
     public partial class App : Application
    {
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
                Log.Fatal(ex, "Failed to initialize ServiceProvider");
                Environment.Exit(1);
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                bool isInitialized = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";
                Log.Information($"Application initialized by watchdog: {isInitialized}");

                var args = Environment.GetCommandLineArgs();
                Log.Information($"Command-line arguments: {string.Join(", ", args)}");

                if (!isInitialized)
                {
                    Log.Warning("Application not initialized by watchdog, showing error window");
                    desktop.MainWindow = new ErrorWindow
                    {
                        DataContext = ActivatorUtilities.CreateInstance<ErrorWindowViewModel>(_serviceProvider)
                    };
                }
                else
                {
                    Log.Information("Showing main window");

                    if (args.Length < 2)
                    {
                        Log.Fatal("No port specified for the UI.");
                        Environment.Exit(1);
                    }

                    int port = int.Parse(args[1]);
                    Log.Information($"Listening on port {port}");
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = ActivatorUtilities.CreateInstance<MainWindowViewModel>(_serviceProvider, port)
                    };
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}