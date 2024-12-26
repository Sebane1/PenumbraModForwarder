using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
using Serilog;

namespace PenumbraModForwarder.UI.Services;

public class ConfigurationListener : IConfigurationListener
{
    private readonly IConfigurationService _configurationService;
    private readonly IXivLauncherService _xivLauncherService;
    private readonly ILogger _logger;

    public ConfigurationListener(IConfigurationService configurationService, IXivLauncherService xivLauncherService)
    {
        _configurationService = configurationService;
        _xivLauncherService = xivLauncherService;
        _logger = Log.ForContext<ConfigurationListener>();
    }

    public void StartListening()
    {
        _logger.Debug("Configuration Listen Events hooked");
        _configurationService.ConfigurationChanged += ConfigurationServiceOnConfigurationChanged;
    }

    private void ConfigurationServiceOnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        _logger.Debug($"Detected change in {e.PropertyName}");

        if (e is {PropertyName: "Common.StartOnFfxivBoot", NewValue: bool shouldAutoStart})
        {
            _xivLauncherService.EnableAutoStartWatchdog(shouldAutoStart);
        }
    }
}