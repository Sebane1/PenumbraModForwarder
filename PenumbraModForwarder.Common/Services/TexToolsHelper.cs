using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class TexToolsHelper : ITexToolsHelper
{
    private readonly IRegistryHelper _registryHelper;
    private readonly IConfigurationService _configurationService;

    public TexToolsHelper(IRegistryHelper registryHelper, IConfigurationService configurationService)
    {
        _registryHelper = registryHelper;
        _configurationService = configurationService;
    }

    public TexToolsStatus SetTexToolConsolePath()
    {
        if ((string)_configurationService.ReturnConfigValue(model => model.TexToolPath) != string.Empty)
        {
            Log.Information("TexTools path already configured");
            return TexToolsStatus.AlreadyConfigured;
        }

        var path = _registryHelper.GetTexToolRegistryValue();
        if (string.IsNullOrEmpty(path))
        {
            Log.Warning("TexTools installation not found in registry");
            return TexToolsStatus.NotInstalled;
        }
    
        // Strip the path of ""
        if (path.StartsWith("\"") && path.EndsWith("\""))
        {
            path = path[1..^1];
        }
    
        var combinedPath = Path.Combine(path, "FFXIV_TexTools", "ConsoleTools.exe");

        if (!File.Exists(combinedPath))
        {
            Log.Warning("ConsoleTools.exe not found at: {Path}", combinedPath);
            return TexToolsStatus.NotFound;
        }

        _configurationService.UpdateConfigValue(config => config.TexToolPath = combinedPath);
        Log.Information("Successfully configured TexTools path: {Path}", combinedPath);
        return TexToolsStatus.Found;
    }
}