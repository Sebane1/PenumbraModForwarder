using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class TexToolsHelper : ITexToolsHelper
{
    private readonly ILogger<TexToolsHelper> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IErrorWindowService _errorWindowService;
    private readonly IRegistryHelper _registryHelper;

    public TexToolsHelper(ILogger<TexToolsHelper> logger, IConfigurationService configurationService, IErrorWindowService errorWindowService, IRegistryHelper registryHelper)
    {
        _logger = logger;
        _configurationService = configurationService;
        _errorWindowService = errorWindowService;
        _registryHelper = registryHelper;
    }

    public void SetTexToolsConsolePath()
    {
        if (_configurationService.GetConfigValue(config => !string.IsNullOrEmpty(config.TexToolPath)))
        {
            _logger.LogInformation("TexTools Console path already set");
            return;
        }
        
        var path = _registryHelper.GetTexToolGetRegistryValue("InstallLocation");
        
        if (string.IsNullOrEmpty(path))
        {
            _logger.LogWarning("TexTools Console not found in registry");
            _configurationService.SetConfigValue((config, texToolPath) => config.TexToolPath = texToolPath, string.Empty);
            return;
        }
        
        // Strip the path of ""
        if (path.StartsWith("\"") && path.EndsWith("\""))
        {
            path = path[1..^1];
        }
        
        var combinedPath = Path.Combine(path, "FFXIV_TexTools", "ConsoleTools.exe");
        
        if (!File.Exists(combinedPath))
        {
            _logger.LogWarning("TexTools Console not found at {TexToolsConsolePath}", combinedPath);
            _configurationService.SetConfigValue((config, texToolPath) => config.TexToolPath = texToolPath, string.Empty);
            return;
        }
        
        _logger.LogInformation("Setting TexTools Console path to {TexToolsConsolePath}", combinedPath);
        
        _configurationService.SetConfigValue((config, texToolPath) => config.TexToolPath = texToolPath, combinedPath);
    }
}