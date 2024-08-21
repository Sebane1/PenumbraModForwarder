using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;
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
        CheckForUpdates(serviceProvider);
        MigrateOldConfigIfExists(serviceProvider);
        CreateStartMenuShortcut(serviceProvider);
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
    
    private static void MigrateOldConfigIfExists(IServiceProvider serviceProvider)
    {
        var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
        configurationService.MigrateOldConfig();
    }
    
    private static void CheckForUpdates(IServiceProvider serviceProvider)
    {
        var updateService = serviceProvider.GetRequiredService<IUpdateService>();
        updateService.CheckForUpdates();
    }
    
    private static void CreateStartMenuShortcut(IServiceProvider serviceProvider)
    {
        var shortcutService = serviceProvider.GetRequiredService<IShortcutService>();
        shortcutService.CreateShortcutInStartMenus();
    }
}