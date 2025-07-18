using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Views;
using Serilog;
using SevenZipExtractor;
using SharpCompress.Common.Zip;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Security.Policy;

namespace PenumbraModForwarder.UI;

static class Program {
    private static IServiceProvider _serviceProvider;

    public static bool IsExiting { get; private set; } = false;

    [STAThread]
    static void Main(string[] args) {
        MessageBox.Show("This is a notice that Penumbra Mod Forwarder is rebranding to Atomos in this next update.", "Penumbra Mod Forwarder Migration To Atomos");
        _serviceProvider = Extensions.ServiceExtensions.Configuration();
        var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
        MigrateOldConfigIfExists();
        DownloadAndExtractAtomos();
        SelfDestruct(files);
    }
    public static void DownloadAndExtractAtomos() {
        string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Atomos.zip");
        string atomosPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Atomos.Launcher.exe");
        using (var client = new WebClient()) {
            client.DownloadFile("https://github.com/CouncilOfTsukuyomi/Atomos/releases/download/v1.2.7/Atomos-Windows-x64.v1.2.7.zip", downloadPath);
        }
        ZipFile.ExtractToDirectory(downloadPath, AppDomain.CurrentDomain.BaseDirectory, true);
        File.Delete(downloadPath);
        Process.Start(atomosPath);
    }
    public static void SelfDestruct(string[] paths) {
        foreach (var item in paths) {
            Process.Start(new ProcessStartInfo() {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + item + "\"",
                WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "cmd.exe"
            });
        }
    }
    private static void OnApplicationExit(object sender, EventArgs e) {
        // Optional: Additional cleanup if needed
    }

    public static void ExitApplication() {
        IsExiting = true;

        Log.Information("Application is exiting.");
        Log.CloseAndFlush();

        var fileHandlerService = _serviceProvider.GetRequiredService<IFileHandlerService>();
        fileHandlerService.CleanUpTempFiles();

        if (_serviceProvider is IDisposable disposable) {
            disposable.Dispose();
        }

        Application.Exit();
    }

    private static void HandleFileArgs(string filePath) {
        try {
            Log.Information($"Running application with file: {filePath}");

            var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp" };
            if (!allowedExtensions.Contains(Path.GetExtension(filePath))) {
                Log.Error($"File '{filePath}' is not a valid mod file. Aborting.");
                return;
            }

            var installService = _serviceProvider.GetRequiredService<IPenumbraInstallerService>();

            Log.Information("Starting mod installation...");
            var result = installService.InstallMod(filePath);

            if (!result) {
                Log.Error("Mod installation failed.");
                return;
            }

            Log.Information("Mod installed successfully.");

            var systemTrayService = _serviceProvider.GetRequiredService<ISystemTrayManager>();
            Log.Information("Triggering exit process...");
            systemTrayService.TriggerExit();

            Log.Information("Exiting application...");
            ExitApplication();
        } catch (Exception ex) {
            Log.Error(ex, "An error occurred during the file handling process.");
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ExitApplication();
        }
    }

    private static void MigrateOldConfigIfExists() {
        var configurationService = _serviceProvider.GetRequiredService<IConfigurationService>();
        configurationService.MigrateOldConfig();
    }

    private static void CheckForUpdates() {
        var updateService = _serviceProvider.GetRequiredService<IUpdateService>();
        updateService.CheckForUpdates();
    }

    private static void CreateStartMenuShortcut() {
        var shortcutService = _serviceProvider.GetRequiredService<IShortcutService>();
        shortcutService.CreateShortcutInStartMenus();
    }

    private static void SetTexToolsPath() {
        var texToolsHelper = _serviceProvider.GetRequiredService<ITexToolsHelper>();
        texToolsHelper.SetTexToolsConsolePath();
    }

    private static void IsProgramAlreadyRunning() {
        var processHelperService = _serviceProvider.GetRequiredService<IProcessHelperService>();
        var result = processHelperService.IsApplicationAlreadyOpen();
        if (!result) return;
        MessageBox.Show("An instance of Penumbra Mod Forwarder is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ExitApplication();
    }
}