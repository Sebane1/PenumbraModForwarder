using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class PenumbraInstallerService : IPenumbraInstallerService
{
    private readonly ILogger<PenumbraInstallerService> _logger;
    private readonly IPenumbraApi _penumbraApi;
    private readonly ISystemTrayManager _systemTrayManager;
    private readonly IConfigurationService _configurationService;
    private readonly IProgressWindowService _progressWindowService;
    private readonly string _dtConversionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\DTConversion";

    public PenumbraInstallerService(ILogger<PenumbraInstallerService> logger, IPenumbraApi penumbraApi, ISystemTrayManager systemTrayManager, IConfigurationService configurationService, IProgressWindowService progressWindowService)
    {
        _logger = logger;
        _penumbraApi = penumbraApi;
        _systemTrayManager = systemTrayManager;
        _configurationService = configurationService;
        _progressWindowService = progressWindowService;

        if (!Directory.Exists(_dtConversionPath))
        {
            Directory.CreateDirectory(_dtConversionPath);
        }
    }
    
    public async Task<bool> InstallMod(string modPath)
    {
        var dtPath = await UpdateToDt(modPath);
        _logger.LogInformation($"Installing mod: {dtPath}");

        var result = await _penumbraApi.InstallAsync(dtPath);
        return result;
    }

    
    private async Task<string> UpdateToDt(string modPath)
    {
        if (!IsConversionNeeded(modPath))
        {
            _logger.LogInformation($"Converted mod already exists: {modPath}");
            return GetConvertedModPath(modPath);
        }

        var textToolPath = _configurationService.GetConfigValue(config => config.TexToolPath);
        if (!string.IsNullOrEmpty(textToolPath) && File.Exists(textToolPath))
        {
            return await ConvertToDt(modPath);
        }

        _logger.LogWarning("TexTools not found. Aborting Conversion.");
        return modPath;
    }
    
    private async Task<string> ConvertToDt(string modPath)
    {
        _logger.LogInformation($"Converting mod to DT using xivModdingFramework: {modPath}");
        var dtPath = GetConvertedModPath(modPath);

        _progressWindowService.ShowProgressWindow();

        try
        {
            var fileName = Path.GetFileName(modPath);

            using var cts = new CancellationTokenSource();
            var progressTask = Task.Run(async () =>
            {
                var progress = 0.0;
                var maxProgress = 88.0;

                while (!cts.Token.IsCancellationRequested && progress < maxProgress)
                {
                    progress += (1.0 - (progress / maxProgress)) * 1.2;
                    progress = Math.Min(progress, maxProgress);

                    _progressWindowService.UpdateProgress(fileName, "Converting to DawnTrail", (int)progress);
                    await Task.Delay(50, cts.Token);
                }
            }, cts.Token);

            var upgradeResult = await xivModdingFramework.Mods.ModpackUpgrader
                .UpgradeModpack(modPath, dtPath, includePartials: true, rewriteOnNoChanges: true);

            cts.Cancel(); // Stop the progress animation
            await progressTask; // Wait for the progress animation to finish

            if (!upgradeResult)
            {
                _logger.LogWarning($"No changes detected or conversion skipped for mod: {modPath}");
                return modPath;
            }

            _progressWindowService.UpdateProgress(fileName, "Conversion Complete", 100);

            _logger.LogInformation($"Mod successfully converted to DT: {dtPath}");
            _systemTrayManager.ShowNotification("Mod Conversion", $"Mod converted to DT: {fileName}");

            File.Delete(modPath);
            _logger.LogInformation($"Deleted original mod: {modPath}");

            return dtPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during mod conversion: {modPath}");
            throw;
        }
        finally
        {
            _progressWindowService.CloseProgressWindow();
        }
    }

    
    private bool IsConversionNeeded(string modPath)
    {
        var convertedModPath = GetConvertedModPath(modPath);
        return !File.Exists(convertedModPath);
    }

    private string GetConvertedModPath(string modPath)
    {
        return Path.Combine(_dtConversionPath, Path.GetFileNameWithoutExtension(modPath) + "_dt" + Path.GetExtension(modPath));
    }

}