using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class TexToolsHelper : ITexToolsHelper
{
    private readonly ILogger<TexToolsHelper> _logger;
    private readonly IVoidToolsEverything _voidToolsEverything;
    private readonly IConfigurationService _configurationService;
    private readonly IErrorWindowService _errorWindowService;

    public TexToolsHelper(ILogger<TexToolsHelper> logger, IVoidToolsEverything voidToolsEverything, IConfigurationService configurationService, IErrorWindowService errorWindowService)
    {
        _logger = logger;
        _voidToolsEverything = voidToolsEverything;
        _configurationService = configurationService;
        _errorWindowService = errorWindowService;
    }

    public void SetTexToolsConsolePath()
    {
        try
        {
            _voidToolsEverything.SetSearch("ConsoleTools.exe");

            if (_voidToolsEverything.Query(true))
            {
                var numResults = _voidToolsEverything.GetNumResults();
                _logger.LogDebug($"Found {numResults} results for ConsoleTools.exe.");

                for (var i = 0; i < numResults; i++)
                {
                    var resultPath = _voidToolsEverything.GetResultFullPathName(i);
                    _logger.LogDebug($"Result {i + 1}: {resultPath}");

                    if (!resultPath.Contains("FFXIV TexTools\\FFXIV_TexTools\\", StringComparison.OrdinalIgnoreCase)
                        || !resultPath.EndsWith("ConsoleTools.exe", StringComparison.OrdinalIgnoreCase)) continue;
                    
                    _logger.LogInformation($"Found ConsoleTools.exe at {resultPath}");
                    _configurationService.SetConfigValue((config, path) => config.TexToolPath = path, resultPath);
                }
            }
            else
            {
                _logger.LogWarning("Query for ConsoleTools.exe failed.");
                _errorWindowService.ShowError("Query for ConsoleTools.exe failed.");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting TexTools path.");
            _errorWindowService.ShowError(e.ToString());
        }
    }
}