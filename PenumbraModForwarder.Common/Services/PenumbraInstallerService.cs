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
    
    public void InstallMod(string modPath)
    {
        var dtPath = UpdateToDt(modPath);
        _logger.LogInformation($"Installing mod: {dtPath}");
        _penumbraApi.InstallAsync(dtPath);
    }
    
    private string UpdateToDt(string modPath)
    {
        if (!IsConversionNeeded(modPath))
        {
            _logger.LogInformation($"Converted mod already exists: {modPath}");
            return GetConvertedModPath(modPath);
        }
        
        var textToolPath = _configurationService.GetConfigValue(config => config.TexToolPath);
        if (!string.IsNullOrEmpty(textToolPath) && File.Exists(textToolPath)) return ConvertToDt(modPath);
        _logger.LogWarning("TexTools not found in registry. Aborting Conversion.");
        return modPath;
    }
    
    private string ConvertToDt(string modPath)
    {
        _logger.LogInformation($"Converting mod to DT: {modPath}");
        var dtPath = GetConvertedModPath(modPath);
        
        _progressWindowService.ShowProgressWindow();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _configurationService.GetConfigValue(config => config.TexToolPath),
                Arguments = $"/upgrade \"{modPath}\" \"{dtPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            var fileName = Path.GetFileName(modPath);
            using (process)
            {
                process.Start();

                var progress = 0.0;  // Use double for smoother transitions
                var maxProgress = 88.0;

                while (!process.HasExited)
                {
                    // Parabolic easing to slow down as we approach maxProgress
                    // The closer progress gets to maxProgress, the smaller the increments become
                    var easedIncrement = (1.0 - (progress / maxProgress)) * 1.2; // Slow down as progress increases

                    progress += easedIncrement;

                    // Cap progress at maxProgress to ensure it never reaches 100%
                    if (progress >= maxProgress)
                    {
                        progress = maxProgress;
                    }

                    _progressWindowService.UpdateProgress(fileName, "Converting to DawnTrail", (int)progress);

                    Thread.Sleep(50); 
                }

                process.WaitForExit();

                if (process.ExitCode != 0 || !File.Exists(dtPath))
                {
                    _logger.LogWarning($"Error converting mod to DT or conversion isn't needed: {modPath}");
                    return modPath;
                }
                
                _progressWindowService.UpdateProgress(fileName, "Conversion Complete", 100);

                _logger.LogInformation($"Mod converted to DT: {dtPath}");
                _systemTrayManager.ShowNotification("Mod Conversion", $"Mod converted to DT: {Path.GetFileName(modPath)}");
                
                File.Delete(modPath);
                _logger.LogInformation($"Deleted original mod: {modPath}");

                return dtPath;
            }
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