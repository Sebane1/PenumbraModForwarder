using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class FileWatcherService : IFileWatcherService
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileWatcher _fileWatcher;
    private readonly IWebSocketServer _webSocketServer;
    private CancellationTokenSource _cancellationTokenSource;

    public FileWatcherService(IConfigurationService configurationService, IFileWatcher fileWatcher, IWebSocketServer webSocketServer)
    {
        _configurationService = configurationService;
        _configurationService.ConfigurationChanged += OnConfigurationChanged;
        _fileWatcher = fileWatcher;
        _webSocketServer = webSocketServer;
        _fileWatcher.FileMoved += OnFileMoved;
    }

    public void Start()
    {
        InitializeFileWatcher();
    }

    public void Stop()
    {
        DisposeFileWatcher();
    }

    private async void InitializeFileWatcher()
    {
        var downloadPaths = _configurationService.ReturnConfigValue(config => config.BackgroundWorker.DownloadPath) as List<string>;

        if (downloadPaths == null || downloadPaths.Count == 0)
        {
            Log.Warning("No download paths specified. FileWatcher will not be initialized.");
            return;
        }
        
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await _fileWatcher.StartWatchingAsync(downloadPaths, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occured while initializing the file watcher.");
        }
    }
    
    private void OnFileMoved(object? sender, FileMovedEvent e)
    {
        Log.Information($"File moved: {e.DestinationPath}");
        var taskId = Guid.NewGuid().ToString();
        var message = WebSocketMessage.CreateStatus(taskId, "Found File", $"Found File: {e.FileName}");
        _webSocketServer.BroadcastToEndpointAsync("/status", message).GetAwaiter().GetResult();
    }
    
    private void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfigurationModel.BackgroundWorker.DownloadPath))
        {
            Log.Information("Configuration changed. Restarting FileWatcher");
            RestartFileWatcher();
        }
    }

    private void RestartFileWatcher()
    {
        DisposeFileWatcher();
        InitializeFileWatcher();
    }

    private void DisposeFileWatcher()
    {
        _cancellationTokenSource.Cancel();
        _fileWatcher.Dispose();
    }

    public void Dispose()
    {
        DisposeFileWatcher();
        _configurationService.ConfigurationChanged -= OnConfigurationChanged;
    }
}