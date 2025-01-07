using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Updater.Extensions;
using Serilog;

namespace PenumbraModForwarder.Updater;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}