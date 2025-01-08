using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Updater.Extensions;
using Serilog;

namespace PenumbraModForwarder.Updater
{
    public static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static string? ExternalVersion { get; set; }

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
                    if (args.Length > 0)
                    {
                        ExternalVersion = args[0];
                    }

                    var services = new ServiceCollection();
                    services.AddApplicationServices();

                    ServiceProvider = services.BuildServiceProvider();
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Application failed to start");
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
}