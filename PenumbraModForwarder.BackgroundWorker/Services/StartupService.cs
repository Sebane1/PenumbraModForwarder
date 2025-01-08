using NLog;
using PenumbraModForwarder.BackgroundWorker.Extensions;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class StartupService : IStartupService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IWebSocketServer _webSocketServer;
    private readonly ITexToolsHelper _texToolsHelper;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly IConfigurationService _configurationService;
    private readonly IConfigurationListener _configurationListener;

    public StartupService(
        IWebSocketServer webSocketServer,
        ITexToolsHelper texToolsHelper,
        IFileWatcherService fileWatcherService, IConfigurationService configurationService, IConfigurationListener configurationListener)
    {
        _webSocketServer = webSocketServer;
        _texToolsHelper = texToolsHelper;
        _fileWatcherService = fileWatcherService;
        _configurationService = configurationService;
        _configurationListener = configurationListener;
    }

    public async Task InitializeAsync()
    {
        if ((bool) _configurationService.ReturnConfigValue(c => c.Common.EnableSentry))
        {
            DependencyInjection.EnableSentryLogging();
        }
        else
        {
            DependencyInjection.DisableSentryLogging();
        }
        
        _logger.Info("Initializing startup checks...");
        await CheckTexToolsInstallation();
        await RunFileWatcherStartupService();
    }

    private async Task RunFileWatcherStartupService()
    {
        _logger.Info("Starting file watcher...");
        await _fileWatcherService.Start();
    }

    private async Task CheckTexToolsInstallation()
    {
        var status = _texToolsHelper.SetTexToolConsolePath();
        var taskId = Guid.NewGuid().ToString();

        var (messageStatus, messageText) = status switch
        {
            TexToolsStatus.AlreadyConfigured => (WebSocketMessageStatus.Completed, (string)null),
            TexToolsStatus.Found => (WebSocketMessageStatus.Completed, "TexTools installation found"),
            TexToolsStatus.NotFound => (WebSocketMessageStatus.Failed, "TexTools ConsoleTools.exe not found"),
            TexToolsStatus.NotInstalled => (WebSocketMessageStatus.Failed, "TexTools installation not found"),
            _ => (WebSocketMessageStatus.Failed, "Unknown error occurred checking TexTools installation")
        };

        if (messageText != null)
        {
            var message = WebSocketMessage.CreateStatus(taskId, messageStatus, messageText);
            await _webSocketServer.BroadcastToEndpointAsync("/status", message);
        }
    }
}