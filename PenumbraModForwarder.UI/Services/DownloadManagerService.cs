using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;

namespace PenumbraModForwarder.UI.Services;

public class DownloadManagerService : IDownloadManagerService
{
    private readonly ILogger _logger;
    private readonly IXmaModDisplay _xmaModDisplay;
    private readonly IAria2Service _aria2Service;
    private readonly IConfigurationService _configurationService;
    private readonly INotificationService _notificationService;

    public DownloadManagerService(
        IXmaModDisplay xmaModDisplay,
        IAria2Service aria2Service,
        IConfigurationService configurationService, 
        INotificationService notificationService)
    {
        _xmaModDisplay = xmaModDisplay;
        _aria2Service = aria2Service;
        _configurationService = configurationService;
        _notificationService = notificationService;
        _logger = Log.ForContext<DownloadManagerService>();
    }

   public async Task DownloadModsAsync(XmaMods mod)
    {
        if (mod == null || string.IsNullOrWhiteSpace(mod.ModUrl))
        {
            _logger.Warning("Cannot download. Mod or its URL is invalid.");
            return;
        }

        var modUri = new Uri(mod.ModUrl);
        if (!modUri.Host.Equals("www.xivmodarchive.com", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Warning("Unsupported domain for download: {Url}", mod.ModUrl);
            _notificationService.ShowNotification(
                $"Unsupported domain '{modUri.Host}'. Only https://www.xivmodarchive.com/ is supported.",
                SoundType.GeneralChime
            );
            return;
        }

        var directLink = await _xmaModDisplay.GetModDownloadLinkAsync(mod.ModUrl);
        if (directLink != null)
        {
            var directUri = new Uri(directLink);
            if (!directUri.Host.Equals("www.xivmodarchive.com", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning("Unsupported download link for download: {Url}", directLink);
                _notificationService.ShowNotification(
                    $"Unsupported download link '{directUri.Host}'. Only https://www.xivmodarchive.com/ is supported.",
                    SoundType.GeneralChime
                );
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(directLink))
        {
            _logger.Warning("No direct link found or an error occurred. Skipping download for {ModName}.", mod.Name);
            return;
        }

        await _notificationService.ShowNotification($"Downloading file {mod.Name}", SoundType.GeneralChime);

        var configuredPaths = _configurationService.ReturnConfigValue(cfg => cfg.BackgroundWorker.DownloadPath)
            as System.Collections.Generic.List<string>;
        if (configuredPaths == null || configuredPaths.Count == 0)
        {
            _logger.Warning("No download path configured. Aborting download for {ModName}.", mod.Name);
            return;
        }

        var downloadPath = configuredPaths.First();
        if (!Directory.Exists(downloadPath))
        {
            Directory.CreateDirectory(downloadPath);
        }

        try
        {
            var result = await _aria2Service.DownloadFileAsync(directLink, downloadPath);
            if (result)
            {
                _logger.Information("Successfully downloaded {ModName} to {Destination}.", mod.Name, downloadPath);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during download of {ModName}.", mod.Name);
        }
    }
}