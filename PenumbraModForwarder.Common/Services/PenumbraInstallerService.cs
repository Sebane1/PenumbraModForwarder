using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class PenumbraInstallerService : IPenumbraInstallerService
{
    private readonly ILogger<PenumbraInstallerService> _logger;
    private readonly IPenumbraApi _penumbraApi;
    private readonly ISystemTrayManager _systemTrayManager;
    private readonly IRegistryHelper _registryHelper;
    private readonly IConfigurationService _configurationService;
    private readonly string _dtConversionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\DTConversion";

    public PenumbraInstallerService(ILogger<PenumbraInstallerService> logger, IPenumbraApi penumbraApi, IRegistryHelper registryHelper, ISystemTrayManager systemTrayManager, IConfigurationService configurationService)
    {
        _logger = logger;
        _penumbraApi = penumbraApi;
        _registryHelper = registryHelper;
        _systemTrayManager = systemTrayManager;
        _configurationService = configurationService;

        if (!Directory.Exists(_dtConversionPath))
        {
            Directory.CreateDirectory(_dtConversionPath);
        }
    }
    
    public void InstallMod(string modPath)
    {
        var dtPath = UpdateToDt(modPath);
        _logger.LogInformation($"Installing mod: {dtPath}");
        var result = _penumbraApi.InstallAsync(dtPath).Result;
        if (!result) return;
        if (_configurationService.GetConfigValue(p => p.AutoDelete))
        {
            DeleteMod(dtPath);
        }

    }

    private void DeleteMod(string modPath)
    {
        while (FileInUse(modPath))
        {
            _logger.LogInformation($"File in use: {modPath}");
            Thread.Sleep(1000);
        }
        File.Delete(modPath);
        _logger.LogInformation($"Deleted mod: {modPath}");
    }
    
    private bool FileInUse(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }
    
    private string UpdateToDt(string modPath)
    {
        if (!IsConversionNeeded(modPath))
        {
            _logger.LogInformation($"Converted mod already exists: {modPath}");
            return GetConvertedModPath(modPath);
        }

        var textToolPath = _registryHelper.GetTexToolsConsolePath();
        if (string.IsNullOrEmpty(textToolPath) || !File.Exists(textToolPath))
        {
            _logger.LogWarning("TexTools not found in registry. Aborting Conversion.");
            return modPath;
        }

        return ConvertToDt(modPath);
    }

    
    private string ConvertToDt(string modPath)
    {
        _logger.LogInformation($"Converting mod to DT: {modPath}");
        _systemTrayManager.ShowNotification("Mod Conversion", $"Converting mod to DT: {Path.GetFileName(modPath)}");
        var dtPath = GetConvertedModPath(modPath);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _registryHelper.GetTexToolsConsolePath(),
                Arguments = $"/upgrade \"{modPath}\" \"{dtPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        using (process)
        {
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0 || !File.Exists(dtPath))
            {
                _logger.LogWarning($"Error converting mod to DT or conversion isn't needed: {modPath}");
                return modPath;
            }

            _logger.LogInformation($"Mod converted to DT: {dtPath}");
            _systemTrayManager.ShowNotification("Mod Conversion", $"Mod converted to DT: {Path.GetFileName(modPath)}");

            // Optionally delete the original mod if conversion was successful
            File.Delete(modPath);
            _logger.LogInformation($"Deleted original mod: {modPath}");

            return dtPath;
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