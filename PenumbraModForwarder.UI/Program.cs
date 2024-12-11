using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Views;
using Serilog;

namespace PenumbraModForwarder.UI;

static class Program
{
    private static IServiceProvider _serviceProvider;

    public static bool IsExiting { get; private set; } = false;

    [STAThread]
    static void Main(string[] args)
    {
        _serviceProvider = Extensions.ServiceExtensions.Configuration();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (args.Length > 0)
        {
            var filePath = args[0];
            HandleFileArgs(filePath);
            return;
        }

        Application.ApplicationExit += OnApplicationExit;

        IsProgramAlreadyRunning();

        CheckForUpdates();

        if (IsExiting)
        {
            return;
        }

        MigrateOldConfigIfExists();
        CreateStartMenuShortcut();
        SetTexToolsPath();
        Application.Run(_serviceProvider.GetRequiredService<MainWindow>());
    }

    private static void OnApplicationExit(object sender, EventArgs e)
    {
        // Optional: Additional cleanup if needed
    }

    public static void ExitApplication()
    {
        IsExiting = true;

        Log.Information("Application is exiting.");
        Log.CloseAndFlush();

        var fileHandlerService = _serviceProvider.GetRequiredService<IFileHandlerService>();
        fileHandlerService.CleanUpTempFiles();

        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Application.Exit();
    }

    private static void HandleFileArgs(string filePath)
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

            var installService = _serviceProvider.GetRequiredService<IPenumbraInstallerService>();

            Log.Information("Starting mod installation...");
            var result = installService.InstallMod(filePath);

            if (!result)
            {
                Log.Error("Mod installation failed.");
                return;
            }

            Log.Information("Mod installed successfully.");

            var systemTrayService = _serviceProvider.GetRequiredService<ISystemTrayManager>();
            Log.Information("Triggering exit process...");
            systemTrayService.TriggerExit();

            Log.Information("Exiting application...");
            ExitApplication();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during the file handling process.");
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitApplication();
        }
    }

    private static void MigrateOldConfigIfExists()
    {
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        configurationService.MigrateOldConfig();
    }

    private static void CheckForUpdates()
    {
        var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
        updateService.CheckForUpdates();
    }

    private static void CreateStartMenuShortcut()
    {
        var shortcutService = _serviceProvider.GetRequiredService<IShortcutService>();
        shortcutService.CreateShortcutInStartMenus();
    }

    private static void SetTexToolsPath()
    {
        var texToolsHelper = _serviceProvider.GetRequiredService<ITexToolsHelper>();
        texToolsHelper.SetTexToolsConsolePath();
    }

    private static void IsProgramAlreadyRunning()
    {
        var processHelperService = _serviceProvider.GetRequiredService<IProcessHelperService>();
        var result = processHelperService.IsApplicationAlreadyOpen();
        if (!result) return;
        MessageBox.Show("An instance of Penumbra Mod Forwarder is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ExitApplication();
    }
}