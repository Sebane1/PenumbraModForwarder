using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services;

public class ArkService : IArkService
{
    private readonly ILogger<ArkService> _logger;
    private readonly IErrorWindowService _errorWindowService;
    private readonly IProcessHelperService _processHelperService;
    private string _cacheFolder;
    private readonly string _arkPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\XIVLauncher\pluginConfigs\RoleplayingVoiceDalamud.json";

    public ArkService(ILogger<ArkService> logger, IErrorWindowService errorWindowService, IProcessHelperService processHelperService)
    {
        _logger = logger;
        _errorWindowService = errorWindowService;
        _processHelperService = processHelperService;
    }
    
    private void CheckArkInstallation()
    {
        if (!File.Exists(_arkPath))
        {
            _logger.LogError("Ark installation not found at {ArkPath}", _arkPath);
            _errorWindowService.ShowError($"Ark installation not found at: {_arkPath}");
        }
        
        var file = File.ReadAllText(_arkPath);
        var config = JsonConvert.DeserializeObject<ArkModel>(file);
        _cacheFolder = config.CacheFolder;
    }

    public void InstallArkFile(string filePath)
    {
        CheckArkInstallation();
        
        if (string.IsNullOrEmpty(_cacheFolder))
        {
            _logger.LogError("Ark cache folder not found in config");
            _errorWindowService.ShowError("Ark cache folder not found in config, this could be because the plugin is not installed.");
            _processHelperService.OpenArk();
        }
        
        var arkFolder = Path.Combine(_cacheFolder, "VoicePack", Path.GetFileNameWithoutExtension(filePath));
        File.Copy(filePath, Path.Combine(arkFolder, Path.GetFileName(filePath)), true);
        File.Delete(filePath);
    }
}