using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Views;
using Serilog;

namespace PenumbraModForwarder.UI;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var serviceProvider = Extensions.ServiceExtensions.Configuration();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ApplicationExit += OnApplicationExit;
        Application.Run(serviceProvider.GetRequiredService<MainWindow>());
    }
    
    private static void OnApplicationExit(object sender, EventArgs e)
    {
        Log.CloseAndFlush();
        
        // Clean up temp files
        var serviceProvider = Extensions.ServiceExtensions.Configuration();
        var fileHandlerService = serviceProvider.GetRequiredService<IFileHandlerService>();
        fileHandlerService.CleanUpTempFiles();
    }
}