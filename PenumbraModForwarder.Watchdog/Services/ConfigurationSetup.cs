using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Watchdog.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Watchdog.Services;

public class ConfigurationSetup : IConfigurationSetup
{
    private readonly IConfigurationService _configurationService;
    public ConfigurationSetup(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public void CreateFiles()
    {
        Log.Information("Creating configuration files");
        _configurationService.CreateConfiguration();
    }
}