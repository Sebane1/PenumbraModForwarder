using PenumbraModForwarder.BackgroundWorker.Extensions;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class ConfigurationListener : IConfigurationListener
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public ConfigurationListener(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _logger = Log.ForContext<ConfigurationListener>();
    }

    private void StartListening()
    {
        _logger.Debug("Configuration Listen Events hooked");
        _configurationService.ConfigurationChanged += ConfigurationServiceOnConfigurationChanged;
    }

    private void ConfigurationServiceOnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        if (e is {PropertyName: "Common.EnableSentry", NewValue: bool shouldEnableSentry})
        {
            if (shouldEnableSentry)
            {
                _logger.Debug("EnableSentry event triggered");
                DependencyInjection.EnableSentryLogging();
            }
            else
            {
                _logger.Debug("DisableSentry event triggered");
                DependencyInjection.DisableSentryLogging();
            }
            
        }
    }
}