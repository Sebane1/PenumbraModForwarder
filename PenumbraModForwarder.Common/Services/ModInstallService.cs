using System.Diagnostics;
using System.Text;
using NLog;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Exceptions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services
{
    public class ModInstallService : IModInstallService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _httpClient;
        private readonly IStatisticService _statisticService;
        private readonly IPenumbraService _penumbraService;
        private readonly IConfigurationService _configurationService;
        private readonly IFileStorage _fileStorage;

        public ModInstallService(
            HttpClient httpClient,
            IStatisticService statisticService,
            IPenumbraService penumbraService,
            IConfigurationService configurationService,
            IFileStorage fileStorage)
        {
            _httpClient = httpClient;
            _statisticService = statisticService;
            _penumbraService = penumbraService;
            _configurationService = configurationService;
            _fileStorage = fileStorage;
        }

        public async Task<bool> InstallModAsync(string path)
        {
            var finalPath = ConvertIfNeeded(path);
            var extension = Path.GetExtension(finalPath);

            if (extension.Equals(".pmp", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _logger.Debug("Using PenumbraService for .pmp mod: {Path}", finalPath);
                    _penumbraService.InitializePenumbraPath();
                    var installedFolderPath = _penumbraService.InstallMod(finalPath);

                    var modName = Path.GetFileName(installedFolderPath);
                    await ReloadModAsync(installedFolderPath, modName);

                    var fileName = Path.GetFileName(finalPath);
                    await _statisticService.RecordModInstallationAsync(fileName);

                    if ((bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.AutoDelete))
                    {
                        _logger.Info("Deleting mod {Path}", finalPath);
                        _fileStorage.Delete(finalPath);
                    }

                    _logger.Info("Mod installed successfully from path '{Path}' via PenumbraService", finalPath);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error installing .pmp mod from path '{Path}'", finalPath);
                    throw new ModInstallException($"Failed to install .pmp mod from path '{finalPath}'.", ex);
                }
            }

            // Fallback for other file types
            var modData = new ModInstallData(finalPath);
            var json = JsonConvert.SerializeObject(modData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.Debug("Sending POST request to {Url}", new Uri(_httpClient.BaseAddress!, "installmod"));
            try
            {
                var response = await _httpClient.PostAsync("installmod", content);
                response.EnsureSuccessStatusCode();

                _logger.Info("Mod installed successfully from path '{Path}'", finalPath);

                var fileName = Path.GetFileName(finalPath);
                await _statisticService.RecordModInstallationAsync(fileName);

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "HTTP request exception while installing mod from path '{Path}'", finalPath);
                throw new ModInstallException($"Failed to install mod from path '{finalPath}'.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected exception while installing mod from path '{Path}'", finalPath);
                throw new ModInstallException($"An unexpected error occurred while installing mod from path '{finalPath}'.", ex);
            }
        }

        private async Task ReloadModAsync(string modFolder, string modName)
        {
            var data = new ModReloadData(modFolder, modName);
            var body = JsonConvert.SerializeObject(data);

            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            _logger.Debug("Posting to /reloadmod for folder '{ModFolder}', name '{ModName}'", modFolder, modName);

            var response = await _httpClient.PostAsync("reloadmod", content);
            response.EnsureSuccessStatusCode();

            // Give Penumbra a little time to refresh
            await Task.Delay(200);
            _logger.Info("Successfully reloaded Penumbra mod at '{ModFolder}' with name '{ModName}'", modFolder, modName);
        }

        private string ConvertIfNeeded(string originalPath)
        {
            var converterPath = (string)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.TexToolPath);
            if (string.IsNullOrWhiteSpace(converterPath) || !File.Exists(converterPath))
            {
                _logger.Info("Conversion tool not found or not configured. Using original path.");
                return originalPath;
            }

            // Create the 'Converted' folder in the same directory as the original file
            var originalDirectory = Path.GetDirectoryName(originalPath) ?? string.Empty;
            var convertedDirectory = Path.Combine(originalDirectory, "Converted");
            Directory.CreateDirectory(convertedDirectory);

            var newFileName = Path.GetFileNameWithoutExtension(originalPath) + "_converted" + Path.GetExtension(originalPath);
            var convertedFilePath = Path.Combine(convertedDirectory, newFileName);

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = converterPath,
                        Arguments = $"/upgrade \"{originalPath}\" \"{convertedFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();

                if (File.Exists(convertedFilePath))
                {
                    _logger.Info("Converted file successfully created at: {Path}", convertedFilePath);

                    if (!(bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.AutoDelete))
                        return convertedFilePath;

                    if (File.Exists(originalPath))
                    {
                        File.Delete(originalPath);
                        _logger.Info("Deleted original mod file at: {Path}", originalPath);
                    }

                    return convertedFilePath;
                }
                else
                {
                    _logger.Warn("No converted file found at: {Path}; using original path.", convertedFilePath);
                    return originalPath;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while converting path '{Path}'. Using original file.", originalPath);
                return originalPath;
            }
        }
    }
}