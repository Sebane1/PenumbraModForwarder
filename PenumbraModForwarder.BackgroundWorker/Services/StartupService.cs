using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Enums;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class StartupService : IStartupService
{
    private readonly IWebSocketServer _webSocketServer;
    private readonly ITexToolsHelper _texToolsHelper;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger _logger;

    public StartupService(IWebSocketServer webSocketServer, ITexToolsHelper texToolsHelper, IFileWatcherService fileWatcherService)
    {
        _webSocketServer = webSocketServer;
        _texToolsHelper = texToolsHelper;
        _fileWatcherService = fileWatcherService;
        _logger = Log.ForContext<StartupService>();
    }

    public async Task InitializeAsync()
    {
        _logger.Information("Initializing startup checks...");
        await CheckTexToolsInstallation();
        await RunFileWatcherStartupService();
    }

    private async Task RunFileWatcherStartupService()
    {
        _logger.Information("Starting file watcher...");
        await _fileWatcherService.Start();
    }

    private async Task CheckTexToolsInstallation()
    {
        var status = _texToolsHelper.SetTexToolConsolePath();
        var taskId = Guid.NewGuid().ToString();
        var (messageStatus, messageText) = status switch
        {
            TexToolsStatus.AlreadyConfigured => (WebSocketMessageStatus.Completed, null),
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