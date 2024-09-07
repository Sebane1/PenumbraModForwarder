using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
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
    static void Main(string[] args)
    {
        var serviceProvider = Extensions.ServiceExtensions.Configuration();
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        if (args.Length > 0)
        {
            // If the argument is --admin, continue with the normal flow
            if (args[0] == "--admin")
            {
                Log.Information("Running application with admin privileges");
            }
            else
            {
                var filePath = args[0];
                HandleFileArgs(serviceProvider, filePath);
            }
        }
        
        IsProgramAlreadyRunning(serviceProvider);
        CheckForUpdates(serviceProvider);
        MigrateOldConfigIfExists(serviceProvider);
        CreateStartMenuShortcut(serviceProvider);
        SetTexToolsPath(serviceProvider);
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
    
    private static void HandleFileArgs(IServiceProvider serviceProvider, string filePath)
    {
        try
        {
            Log.Information($"Running application with file: {filePath}");
    
            var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp" };
            if (!allowedExtensions.Contains(Path.GetExtension(filePath)))
            {
                Log.Error($"File '{filePath}' is not a valid mod file. Aborting.");
                return;
            }

            var installService = serviceProvider.GetRequiredService<IPenumbraInstallerService>();
    
            Log.Information("Starting mod installation...");
            var result = installService.InstallMod(filePath);

            if (!result)
            {
                Log.Error("Mod installation failed.");
                return;
            }
        
            Log.Information("Mod installed successfully.");
        
            var systemTrayService = serviceProvider.GetRequiredService<ISystemTrayManager>();
            Log.Information("Triggering exit process...");
            systemTrayService.TriggerExit();
        
            Log.Information("Exiting application...");
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during the file handling process.");
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
    
    private static void SetTexToolsPath(IServiceProvider serviceProvider)
    {
        var texToolsHelper = serviceProvider.GetRequiredService<ITexToolsHelper>();
        texToolsHelper.SetTexToolsConsolePath();
    }

    private static void IsProgramAlreadyRunning(IServiceProvider serviceProvider)
    {
        var processHelperService = serviceProvider.GetRequiredService<IProcessHelperService>();
        var result = processHelperService.IsApplicationAlreadyOpen();
        if (!result) return;
        MessageBox.Show("An instance of Penumbra Mod Forwarder is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(0);
    }
}