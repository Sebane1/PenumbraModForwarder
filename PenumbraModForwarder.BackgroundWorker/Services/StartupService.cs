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

    public StartupService(IWebSocketServer webSocketServer, ITexToolsHelper texToolsHelper)
    {
        _webSocketServer = webSocketServer;
        _texToolsHelper = texToolsHelper;
    }

    public async Task InitializeAsync()
    {
        Log.Information("Initializing startup checks...");
        await CheckTexToolsInstallation();
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
    
    /// <summary>
    /// This is used for Testing if the Progress bar is working correctly
    /// </summary>
    public async Task SimulateProgress()
    {
        var taskId = Guid.NewGuid().ToString();
    
        for (int i = 0; i <= 100; i += 10)
        {
            var message = WebSocketMessage.CreateProgress(
                taskId,
                i,
                $"Processing..."
            );
        
            await _webSocketServer.BroadcastToEndpointAsync("/status", message);
            await Task.Delay(500); // Delay to simulate work
        }

        // Send completion message
        var completionMessage = WebSocketMessage.CreateStatus(
            taskId,
            WebSocketMessageStatus.Completed,
            "Process completed successfully"
        );
    
        await _webSocketServer.BroadcastToEndpointAsync("/status", completionMessage);
    }
}