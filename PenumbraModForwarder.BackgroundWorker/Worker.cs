using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker;

public class Worker : BackgroundService
{
    private readonly IWebSocketServer _webSocketServer;

    public Worker(IWebSocketServer webSocketServer)
    {
        _webSocketServer = webSocketServer;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting WebSocket Server...");
        _webSocketServer.Start();
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Stopping WebSocket Server...");
        return base.StopAsync(cancellationToken);
    }
}