using System;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.UI.Extensions;
using Serilog;

namespace PenumbraModForwarder.UI
{
    public class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static IConfiguration Configuration { get; private set; } = null!;

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
                    Configuration = new ConfigurationBuilder()
                        .AddUserSecrets<Program>()
                        .AddEnvironmentVariables()
                        .Build();
                    
                    var sentryDsn = Configuration["SENTRY_DSN"];
                    if (string.IsNullOrWhiteSpace(sentryDsn))
                    {
                        Console.WriteLine("SENTRY_DSN is not provided. Sentry logging will not be configured.");
                    }
                    
                    var services = new ServiceCollection();
                    services.AddApplicationServices();
                    services.SetupLogging(sentryDsn);

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