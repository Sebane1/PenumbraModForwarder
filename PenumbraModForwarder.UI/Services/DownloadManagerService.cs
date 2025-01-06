using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;

namespace PenumbraModForwarder.UI.Services
{
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

        public async Task DownloadModsAsync(XmaMods mod, CancellationToken ct)
        {
            if (mod?.ModUrl is not { Length: > 0 })
            {
                _logger.Warning("Cannot download. `mod` or `mod.ModUrl` is invalid.");
                return;
            }

            var modUri = new Uri(mod.ModUrl);
            if (!IsXivModArchiveDomain(modUri))
            {
                _logger.Warning("Unsupported domain for download: {Url}", mod.ModUrl);
                await _notificationService.ShowNotification(
                    $"Unsupported domain '{modUri.Host}'. Only https://www.xivmodarchive.com/ is supported.",
                    SoundType.GeneralChime
                );
                return;
            }
            
            var directLink = await _xmaModDisplay.GetModDownloadLinkAsync(mod.ModUrl /*, ct if applicable */);
            if (string.IsNullOrWhiteSpace(directLink))
            {
                _logger.Warning("No direct link found or an error occurred. Skipping download for {Name}.", mod.Name);
                return;
            }

            var directUri = new Uri(directLink);
            if (!IsXivModArchiveDomain(directUri))
            {
                _logger.Warning("Unsupported download link for download: {Url}", directLink);
                await _notificationService.ShowNotification(
                    $"Unsupported download link '{directUri.Host}'. Only https://www.xivmodarchive.com/ is supported.",
                    SoundType.GeneralChime
                );
                return;
            }

            await _notificationService.ShowNotification($"Downloading file: {mod.Name}", SoundType.GeneralChime);

            var configuredPaths = _configurationService.ReturnConfigValue(cfg => cfg.BackgroundWorker.DownloadPath)
                as System.Collections.Generic.List<string>;
            if (configuredPaths is null || !configuredPaths.Any())
            {
                _logger.Warning("No download path configured. Aborting download for {Name}.", mod.Name);
                return;
            }

            var downloadPath = configuredPaths.First();
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            try
            {
                var result = await _aria2Service.DownloadFileAsync(directLink, downloadPath, ct);
                if (result)
                {
                    _logger.Information(
                        "Successfully downloaded {Name} to {Destination}", 
                        mod.Name, 
                        downloadPath
                    );
                }
                else
                {
                    _logger.Warning("Download of {Name} did not complete successfully.", mod.Name);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Download canceled for {Name}.", mod.Name);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during download of {Name}.", mod.Name);
            }
        }

        private static bool IsXivModArchiveDomain(Uri uri) =>
            uri.Host.Equals("www.xivmodarchive.com", StringComparison.OrdinalIgnoreCase);
    }
}