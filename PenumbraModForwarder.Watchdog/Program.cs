using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Watchdog.Extensions;
using PenumbraModForwarder.Watchdog.Imports;
using PenumbraModForwarder.Watchdog.Interfaces;
using PenumbraModForwarder.Watchdog.Services;
using Serilog;

namespace PenumbraModForwarder.Watchdog;

class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddApplicationServices();
        
        var serviceProvider = services.BuildServiceProvider();

        var configService = serviceProvider.GetService<IConfigurationSetup>();
        
        configService.CreateFiles();
        
        // Hide the console window on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HideConsoleWindow();
        }
        
        var processManager = serviceProvider.GetService<IProcessManager>();
        processManager.Run();
    }

    static void HideConsoleWindow()
    {
        var handle = DllImports.GetConsoleWindow();
        if (handle != IntPtr.Zero)
        {
            Log.Information("Hiding console window");
            DllImports.ShowWindow(handle, DllImports.SW_HIDE);
            Log.Information("Console window should now be hidden.");
        }
    }
}