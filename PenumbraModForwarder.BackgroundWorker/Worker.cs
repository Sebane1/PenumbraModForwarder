using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker;

public class Worker : BackgroundService
{
    private readonly IWebSocketServer _webSocketServer;
    private readonly IStartupService _startupService;
    private readonly int _port;
    private bool _initialized;

    public Worker(IWebSocketServer webSocketServer, IStartupService startupService, int port)
    {
        _webSocketServer = webSocketServer;
        _startupService = startupService;
        _port = port;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting WebSocket Server...");
        _webSocketServer.Start(_port);
        
        Log.Information("Starting FileWatcher...");
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_initialized && _webSocketServer.HasConnectedClients())
                {
                    await _startupService.InitializeAsync();
                    _initialized = true;
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Worker stopping...");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred in worker");
            throw;
        }
    }
}