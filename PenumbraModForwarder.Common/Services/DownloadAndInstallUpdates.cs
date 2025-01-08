using System.Runtime.InteropServices;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services
{
    public class DownloadAndInstallUpdates : IDownloadAndInstallUpdates
    {
        private readonly ILogger _logger;
        private readonly IAria2Service _aria2Service;
        private readonly IUpdateService _updateService;

        public DownloadAndInstallUpdates(IAria2Service aria2Service, IUpdateService updateService)
        {
            _aria2Service = aria2Service;
            _updateService = updateService;
            _logger = Log.ForContext<DownloadAndInstallUpdates>();
        }


        /// <summary>
        /// Checks for a newer release, downloads any OS-compatible .zip file(s) to a temp directory,
        /// and calls ExtractZipsAsync for final installation. Returns a tuple:
        /// (true, path) if successful, otherwise (false, "").
        /// </summary>
        /// <param name="currentVersion">The currently installed version (e.g. "1.0.0").</param>
        /// <returns>(success, downloadPath)</returns>
        public async Task<(bool success, string downloadPath)> DownloadAndInstallAsync(string currentVersion)
        {
            try
            {
                _logger.Information("Checking if update is needed for version {Version}", currentVersion);

                var needsUpdate = await _updateService.NeedsUpdateAsync(currentVersion);
                if (!needsUpdate)
                {
                    _logger.Information("No update needed. Current version is up-to-date.");
                    return (false, string.Empty);
                }

                var tempDir = Path.Combine(Path.GetTempPath(), "PenumbraModForwarder", "Updates");
                Directory.CreateDirectory(tempDir);

                var zipUrls = await _updateService.GetUpdateZipLinksAsync(currentVersion);
                if (zipUrls.Count == 0)
                {
                    _logger.Warning("No .zip assets found for the latest release. Update cannot proceed.");
                    return (false, string.Empty);
                }

                var osFilteredUrls = zipUrls.Where(IsOsCompatibleAsset).ToList();
                if (osFilteredUrls.Count == 0)
                {
                    _logger.Warning("No assets matching the current OS were found.");
                    return (false, string.Empty);
                }

                var downloadedPaths = new List<string>();
                foreach (var url in osFilteredUrls)
                {
                    _logger.Information("Starting download for {Url}", url);

                    var success = await _aria2Service.DownloadFileAsync(url, tempDir, CancellationToken.None);
                    if (!success)
                    {
                        _logger.Error("Failed to download {Url}. Aborting update.", url);
                        return (false, string.Empty);
                    }

                    var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                    var finalPath = Path.Combine(tempDir, Uri.UnescapeDataString(fileName));
                    downloadedPaths.Add(finalPath);
                }

                foreach (var zipPath in downloadedPaths)
                {
                    var extractOk = await ExtractZipsAsync(zipPath, tempDir, CancellationToken.None);
                    if (!extractOk)
                    {
                        _logger.Error("Failed to extract {ZipPath}. Aborting update.", zipPath);
                        return (false, string.Empty);
                    }
                }

                _logger.Information("All OS-compatible .zip assets have been downloaded and extracted to {Directory}", tempDir);
                return (true, tempDir);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while downloading and installing updates.");
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// Extracts a given zip file into the specified directory, then removes the zip file.
        /// Uses the custom ArchiveFile class for extraction.
        /// </summary>
        /// <param name="zipPath">Path to the zip file.</param>
        /// <param name="extractFolder">Folder to which the contents are extracted.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if extraction succeeded, otherwise false.</returns>
        private async Task<bool> ExtractZipsAsync(string zipPath, string extractFolder, CancellationToken ct)
        {
            try
            {
                if (!File.Exists(zipPath))
                {
                    _logger.Warning("Zip file {ZipPath} not found.", zipPath);
                    return false;
                }

                // Mimic some async boundary
                await Task.Yield();

                using (var archiveFile = new SevenZipExtractor.ArchiveFile(zipPath))
                {
                    // This extracts all files to the specified folder, overwriting if needed
                    archiveFile.Extract(extractFolder, overwrite: true);
                }

                // Delete the zip after extraction if desired
                File.Delete(zipPath);
                _logger.Information("Extracted and deleted {ZipPath}", zipPath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to extract zip file at {ZipPath}", zipPath);
                return false;
            }
        }

        /// <summary>
        /// Determines if the asset URL contains an OS-specific token (e.g., Winx64 or Linux).
        /// </summary>
        private bool IsOsCompatibleAsset(string assetUrl)
        {
            var fileName = assetUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

            // For Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return fileName.Contains("Winx64", StringComparison.OrdinalIgnoreCase);

            // For Linux-like
            return fileName.Contains("Linux", StringComparison.OrdinalIgnoreCase);
        }
    }
}