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
        var dtPath = Path.Combine(_dtConversionPath, Path.GetFileName(modPath));
        
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
            
            if (process.ExitCode != 0)
            {
                _logger.LogWarning($"Error/Mod doesn't need converting mod to DT: {modPath}");
                return modPath;
            }
            
            _logger.LogInformation($"Mod converted to DT: {dtPath}");
            
            // Delete the original mod
            File.Delete(modPath);
            
            return dtPath;
        }
    }
}