using System.Reflection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.Services;

public class StartupService : IStartupService
{
    private readonly ILogger<StartupService> _logger;
    private readonly IRegistryHelper _registryHelper;
    private readonly IConfigurationService _configurationService;
    private readonly IErrorWindowService _errorWindowService;
    private const string appName = "Penumbra Mod Forwarder";

    public StartupService(ILogger<StartupService> logger, IRegistryHelper registryHelper, IConfigurationService configurationService, IErrorWindowService errorWindowService)
    {
        _logger = logger;
        _registryHelper = registryHelper;
        _configurationService = configurationService;
        _errorWindowService = errorWindowService;
    }

    public void RunOnStartup()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PenumbraModForwarder.exe");
        
        try
        {
            if (!_configurationService.GetConfigValue(c => c.StartOnBoot))
            {
                _logger.LogInformation("Removing application from startup");
                RemoveApplicationFromStartup();
                return;
            }
            
            _logger.LogInformation("Adding application to startup");
            AddApplicationToStartup(path);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to run on startup");
            _errorWindowService.ShowError(e.ToString());
        }
    }

    private void AddApplicationToStartup(string appPath)
    {
        _registryHelper.AddApplicationToStartup(appName, appPath);
    }
    
    private void RemoveApplicationFromStartup()
    {
        _registryHelper.RemoveApplicationFromStartup(appName);
    }
}