using System.Runtime.InteropServices;
using PenumbraModForwarder.Watchdog.Imports;
using PenumbraModForwarder.Watchdog.Services;
using Serilog;

namespace PenumbraModForwarder.Watchdog;

class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/Watchdog.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        
        // Hide the console window on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HideConsoleWindow();
        }

        var processManager = new ProcessManager();
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