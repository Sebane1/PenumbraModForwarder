using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class FileWatcherStartupService
{
    private readonly IConfigurationService _configurationService;

    public FileWatcherStartupService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task InitializeAsync()
    {
        _configurationService.ConfigurationChanged += ConfigChange;
    }

    private void ConfigChange(object? sender, EventArgs e)
    {
        Log.Information("Configuration Has been changed");
    }
}