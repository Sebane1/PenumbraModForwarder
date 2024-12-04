using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.UI.Extensions;
using PenumbraModForwarder.Common.Services;
using Serilog;

namespace PenumbraModForwarder.UI;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static void Main(string[] args)
    {
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
            Log.Fatal(ex, "Application failed to start");
            Environment.Exit(1);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}