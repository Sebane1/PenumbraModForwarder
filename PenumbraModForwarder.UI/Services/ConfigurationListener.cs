using System.Runtime.InteropServices;
using NLog;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Extensions;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.Services;

public class ConfigurationListener : IConfigurationListener
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IConfigurationService _configurationService;
    private readonly IXivLauncherService _xivLauncherService;
    private readonly IFileLinkingService _fileLinkingService;

    public ConfigurationListener(
        IConfigurationService configurationService,
        IXivLauncherService xivLauncherService,
        IFileLinkingService fileLinkingService)
    {
        _configurationService = configurationService;
        _xivLauncherService = xivLauncherService;
        _fileLinkingService = fileLinkingService;

        StartListening();
    }

    private void StartListening()
    {
        // Replaces Serilog's _logger.Debug(...) with NLog's
        _logger.Debug("Configuration Listen Events hooked");

        _configurationService.ConfigurationChanged += ConfigurationServiceOnConfigurationChanged;
    }

    private void ConfigurationServiceOnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        _logger.Debug($"Detected change in {e.PropertyName}");

        if (e is { PropertyName: "Common.StartOnFfxivBoot", NewValue: bool shouldAutoStart })
        {
            _xivLauncherService.EnableAutoStartWatchdog(shouldAutoStart);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        if (e is { PropertyName: "Common.FileLinkingEnabled", NewValue: bool shouldLinkFiles })
        {
            if (shouldLinkFiles)
            {
                _fileLinkingService.EnableFileLinking();
            }
            else
            {
                _fileLinkingService.DisableFileLinking();
            }
        }

        if (e is { PropertyName: "Common.StartOnBoot", NewValue: bool shouldStartOnBoot })
        {
            if (shouldStartOnBoot)
            {
                _fileLinkingService.EnableStartup();
            }
            else
            {
                _fileLinkingService.DisableStartup();
            }
        }

        if (e is { PropertyName: "Common.EnableSentry", NewValue: bool shouldEnableSentry })
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