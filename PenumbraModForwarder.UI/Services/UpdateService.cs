using Microsoft.Extensions.Logging;
using PenumbraModForwarder.UI.Interfaces;
using AutoUpdaterDotNET;

namespace PenumbraModForwarder.UI.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly string _updateUrl = "https://raw.githubusercontent.com/Sebane1/PenumbraModForwarder/master/update.xml";

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;

        AutoUpdater.ApplicationExitEvent += OnApplicationExit;
        // AutoUpdater.DownloadPath = Application.StartupPath;
        AutoUpdater.Synchronous = true;
        AutoUpdater.Mandatory = true;
        AutoUpdater.UpdateMode = Mode.ForcedDownload;
        AutoUpdater.ClearAppDirectory = true;

        AutoUpdater.InstalledVersion = GetInstalledVersion();
    }

    public void CheckForUpdates()
    {
        _logger.LogInformation("Checking for updates...");
        _logger.LogInformation($"Current version: {AutoUpdater.InstalledVersion}");

        AutoUpdater.Start(_updateUrl);
    }

    private Version GetInstalledVersion()
    {
        var versionString = Application.ProductVersion.Split("+")[0];
        return new Version(versionString);
    }

    private void OnApplicationExit()
    {
        _logger.LogInformation("Application is exiting due to an update");
        Program.ExitApplication();
    }
}