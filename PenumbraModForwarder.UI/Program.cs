using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.UI.Extensions;

namespace PenumbraModForwarder.UI;

public class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        bool isNewInstance;
        using (var mutex = new Mutex(true, "PenumbraModForwarder.UI", out isNewInstance))
        {
            if (!isNewInstance)
            {
                Console.WriteLine("Another instance of PenumbraModForwarder.UI is already running. Exiting...");
                return;
            }

#if DEBUG
            // In debug mode, append a default port if none is provided
            if (args.Length == 0)
            {
                args = new string[] { "12345" }; // Default port for debugging
            }
#endif

            try
            {
                var services = new ServiceCollection();
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