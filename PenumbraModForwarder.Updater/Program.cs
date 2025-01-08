using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.Updater.Extensions;
using PenumbraModForwarder.Updater.Interfaces;
using PenumbraModForwarder.Updater.Services;

namespace PenumbraModForwarder.Updater;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        bool isNewInstance;
        using (var mutex = new Mutex(true, "PenumbraModForwarder.Updater", out isNewInstance))
        {
            if (!isNewInstance)
            {
                Console.WriteLine("Another instance of PenumbraModForwarder.Updater is already running. Exiting...");
                return;
            }

            try
            {
                var services = new ServiceCollection();

                // Create and register your app arguments
                var appArgs = new AppArguments(args);
                services.AddSingleton<IAppArguments>(appArgs);

                // Register other application services
                services.AddApplicationServices();

                ServiceProvider = services.BuildServiceProvider();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Application failed to start");
                Environment.Exit(1);
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}