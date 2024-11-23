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
        var fileName = Path.GetFileName(modPath);
        var dtPath = GetConvertedModPath(modPath);
        
        _logger.LogInformation($"Starting DT conversion for mod: {fileName}");
        _logger.LogDebug($"Source path: {modPath}");
        _logger.LogDebug($"Target path: {dtPath}");

        _progressWindowService.ShowProgressWindow();

        try
        {
            // Validate input path
            if (!File.Exists(modPath))
            {
                throw new FileNotFoundException($"Source mod file not found: {modPath}");
            }

            // Create output directory if needed
            var outputDir = Path.GetDirectoryName(dtPath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

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

            // Get mod info before upgrade
            var modInfo = await xivModdingFramework.Mods.FileTypes.TTMP.GetModpackInfo(modPath);
            _logger.LogInformation($"Processing mod: {modInfo.ModPack.Name}");

            // Perform the upgrade
            var upgradeResult = await xivModdingFramework.Mods.ModpackUpgrader
                .UpgradeModpack(modPath, dtPath, includePartials: true, rewriteOnNoChanges: true);

            cts.Cancel();
            await progressTask;

            if (!upgradeResult)
            {
                _logger.LogWarning($"No changes detected for mod: {fileName}");
                _progressWindowService.UpdateProgress(fileName, "No Changes Required", 100);
                return modPath;
            }

            _progressWindowService.UpdateProgress(fileName, "Conversion Complete", 100);
            _logger.LogInformation($"Mod successfully converted to DT: {dtPath}");
            
            try
            {
                File.Delete(modPath);
                _logger.LogInformation($"Deleted original mod: {modPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to delete original mod file: {modPath}");
                // Continue execution since this is not a critical failure
            }

            _systemTrayManager.ShowNotification("Mod Conversion", $"Mod converted to DT: {fileName}");
            return dtPath;
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            if (ex.Message.Contains("An error occurred while updating Group"))
            {
                // Extract the group name from the error message for better logging
                var groupMatch = System.Text.RegularExpressions.Regex.Match(ex.Message, @"updating Group: (.*?) -");
                if (groupMatch.Success)
                {
                    var groupName = groupMatch.Groups[1].Value;
                    _logger.LogError(ex, $"Failed to convert group '{groupName}' in mod: {fileName}");
                }
            }
            
            _logger.LogError(ex, $"Failed to convert mod: {fileName}");
            _progressWindowService.UpdateProgress(fileName, "Conversion Failed", 0);
            throw new Exception($"Failed to convert mod {fileName}: {errorMessage}", ex);
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