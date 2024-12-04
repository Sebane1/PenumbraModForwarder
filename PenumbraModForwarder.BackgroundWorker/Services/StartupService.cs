using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Common.Enums;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class StartupService : IStartupService
{
    private readonly IWebSocketServer _webSocketServer;
    private readonly ITexToolsHelper _texToolsHelper;
    private readonly IFileWatcherStartupService _fileWatcherStartupService;

    public StartupService(IWebSocketServer webSocketServer, ITexToolsHelper texToolsHelper, IFileWatcherStartupService fileWatcherStartupService)
    {
        _webSocketServer = webSocketServer;
        _texToolsHelper = texToolsHelper;
        _fileWatcherStartupService = fileWatcherStartupService;
    }

    public async Task InitializeAsync()
    {
        Log.Information("Initializing startup checks...");
        await CheckTexToolsInstallation();
        await RunFileWatcherStartupService();
    }

    private async Task RunFileWatcherStartupService()
    {
        Log.Information("Starting file watcher..");
        _fileWatcherStartupService.Start();
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