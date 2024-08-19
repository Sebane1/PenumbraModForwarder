using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class PenumbraInstallerService : IPenumbraInstallerService
{
    private readonly ILogger<PenumbraInstallerService> _logger;
    private readonly IPenumbraApi _penumbraApi;
    private readonly IRegistryHelper _registryHelper;
    private readonly string _dtConversionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\DTConversion";

    public PenumbraInstallerService(ILogger<PenumbraInstallerService> logger, IPenumbraApi penumbraApi, IRegistryHelper registryHelper)
    {
        _logger = logger;
        _penumbraApi = penumbraApi;
        _registryHelper = registryHelper;
        
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

        var textToolPath = _registryHelper.GetTexToolsConsolePath();
        if (string.IsNullOrEmpty(textToolPath) || !File.Exists(textToolPath))
        {
            _logger.LogWarning("TexTools not found in registry. Aborting Conversion.");
            return modPath;
        }

        _logger.LogInformation($"Converting mod to DT: {modPath}");
        return ConvertToDt(modPath);
    }

    
    private string ConvertToDt(string modPath)
    {
        _logger.LogInformation($"Converting mod to DT: {modPath}");
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

            // Optionally delete the original mod if conversion was successful
            File.Delete(modPath);

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