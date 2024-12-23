using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class FileWatcherService : IFileWatcherService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IWebSocketServer _webSocketServer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IModHandlerService _modHandlerService;
    private IFileWatcher _fileWatcher;
    private bool _eventsSubscribed = false;

    public FileWatcherService(
        IConfigurationService configurationService,
        IWebSocketServer webSocketServer,
        IServiceProvider serviceProvider,
        IModHandlerService modHandlerService)
    {
        _logger = Log.ForContext<FileWatcherService>();
        _configurationService = configurationService;
        _webSocketServer = webSocketServer;
        _serviceProvider = serviceProvider;
        _modHandlerService = modHandlerService;
        _configurationService.ConfigurationChanged += OnConfigurationChanged;
    }

    public async Task Start()
    {
        await InitializeFileWatcherAsync();
    }

    public void Stop()
    {
        DisposeFileWatcher();
    }

    private async Task InitializeFileWatcherAsync()
    {
        _logger.Debug("Initializing FileWatcher...");
        var downloadPaths = (_configurationService.ReturnConfigValue(
            config => config.BackgroundWorker.DownloadPath
        ) as List<string>)?.Distinct().ToList();

        if (downloadPaths == null || downloadPaths.Count == 0)
        {
            _logger.Warning("No download paths specified. FileWatcher will not be initialized.");
            return;
        }

        try
        {
            _logger.Debug("Resolving new IFileWatcher instance...");
            _fileWatcher = _serviceProvider.GetRequiredService<IFileWatcher>();
            if (!_eventsSubscribed)
            {
                _fileWatcher.FileMoved += OnFileMoved;
                _fileWatcher.FilesExtracted += OnFilesExtracted;
                _eventsSubscribed = true;
                _logger.Debug("Event handlers attached.");
            }

            _logger.Debug("Starting watchers for the following paths:");
            foreach (var downloadPath in downloadPaths)
            {
                _logger.Debug(" - {DownloadPath}", downloadPath);
            }

            await _fileWatcher.StartWatchingAsync(downloadPaths);
            _logger.Debug("FileWatcher started successfully.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while initializing the file watcher.");
        }
    }

    private async Task RestartFileWatcherAsync()
    {
        _logger.Debug("Restarting FileWatcher...");
        DisposeFileWatcher();
        await InitializeFileWatcherAsync();
        _logger.Debug("FileWatcher restarted successfully.");
    }

    private void DisposeFileWatcher()
    {
        if (_fileWatcher != null)
        {
            if (_eventsSubscribed)
            {
                _fileWatcher.FileMoved -= OnFileMoved;
                _fileWatcher.FilesExtracted -= OnFilesExtracted;
                _eventsSubscribed = false;
            }
            _fileWatcher.Dispose();
            _fileWatcher = null;
        }
    }

    private async void OnConfigurationChanged(object? sender, ConfigurationChangedEventArgs e)
    {
        try
        {
            _logger.Debug("Configuration: {PropertyName} changed to: {NewValue}", e.PropertyName, e.NewValue);
            if (e.PropertyName == "BackgroundWorker.DownloadPath")
            {
                _logger.Information("Configuration changed. Restarting FileWatcher");
                await RestartFileWatcherAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred in OnConfigurationChanged.");
        }
    }

    private void OnFileMoved(object? sender, FileMovedEvent e)
    {
        _logger.Information("File moved: {DestinationPath}", e.DestinationPath);
        var taskId = Guid.NewGuid().ToString();
        var message = WebSocketMessage.CreateStatus(taskId, "Found File", $"Found File: {e.FileName}");
        _webSocketServer.BroadcastToEndpointAsync("/status", message).GetAwaiter().GetResult();
        _modHandlerService.HandleFileAsync(e.DestinationPath).GetAwaiter().GetResult();
    }

    private void OnFilesExtracted(object? sender, FilesExtractedEventArgs e)
    {
        _logger.Information("Files extracted from archive: {ArchiveFileName}", e.ArchiveFileName);

        var taskId = Guid.NewGuid().ToString();
        var message = WebSocketMessage.CreateStatus(
            taskId,
            "Extracted Files",
            $"Extracted {e.ExtractedFilePaths.Count} files from {e.ArchiveFileName}"
        );
        _webSocketServer.BroadcastToEndpointAsync("/status", message).GetAwaiter().GetResult();

        foreach (var filePath in e.ExtractedFilePaths)
        {
            _logger.Information("Processing extracted file: {FilePath}", filePath);
            _modHandlerService.HandleFileAsync(filePath).GetAwaiter().GetResult();
        }
    }

    public void Dispose()
    {
        DisposeFileWatcher();
        _configurationService.ConfigurationChanged -= OnConfigurationChanged;
    }
}