using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Events;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using ILogger = Serilog.ILogger;
using System.Collections.Concurrent;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.BackgroundWorker.Events;

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

    // Dictionary to track pending user selections
    private readonly ConcurrentDictionary<string, TaskCompletionSource<List<string>>> _pendingSelections = new();

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

        // Subscribe to incoming messages
        _webSocketServer.MessageReceived += OnWebSocketMessageReceived;
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
    var installAll = (bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.InstallAll);
    var deleteUnselected = (bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.AutoDelete);

    if (!installAll)
    {
        // Serialize the list of extracted file paths into JSON
        var extractedFilesJson = JsonConvert.SerializeObject(e.ExtractedFilePaths);

        // Create a message to ask the user which files to install
        var message = new WebSocketMessage
        {
            Type = WebSocketMessageType.Status,
            TaskId = taskId,
            Status = "select_files",
            Progress = 0,
            Message = extractedFilesJson
        };
        
        _webSocketServer.BroadcastToEndpointAsync("/install", message).GetAwaiter().GetResult();

        // Wait for user response
        var selectedFiles = WaitForUserSelection(taskId);

        if (selectedFiles != null && selectedFiles.Any())
        {
            foreach (var selectedFile in selectedFiles)
            {
                _modHandlerService.HandleFileAsync(selectedFile).GetAwaiter().GetResult();
            }
            
            if (deleteUnselected)
            {
                var unselectedFiles = e.ExtractedFilePaths.Except(selectedFiles).ToList();
                foreach (var unselectedFile in unselectedFiles)
                {
                    try
                    {
                        if (File.Exists(unselectedFile))
                        {
                            _logger.Information("Deleting unselected file: {Path}", unselectedFile);
                            File.Delete(unselectedFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error deleting unselected file: {Path}", unselectedFile);
                    }
                }
            }

            var completionMessage = WebSocketMessage.CreateStatus(
                taskId,
                WebSocketMessageStatus.Completed,
                "Selected files have been installed."
            );
            _webSocketServer.BroadcastToEndpointAsync("/install", completionMessage).GetAwaiter().GetResult();
        }
        else
        {
            _logger.Information("No files selected for installation.");
            
            if (deleteUnselected)
            {
                foreach (var extractedFile in e.ExtractedFilePaths)
                {
                    try
                    {
                        if (File.Exists(extractedFile))
                        {
                            _logger.Information("Deleting unselected file: {Path}", extractedFile);
                            File.Delete(extractedFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error deleting file: {Path}", extractedFile);
                    }
                }
            }

            var completionMessage = WebSocketMessage.CreateStatus(
                taskId,
                WebSocketMessageStatus.Completed,
                "No files were selected for installation."
            );
            _webSocketServer.BroadcastToEndpointAsync("/install", completionMessage).GetAwaiter().GetResult();
        }
    }
    else
    {
        // If InstallAll is true, process all extracted files
        var message = WebSocketMessage.CreateStatus(
            taskId,
            WebSocketMessageStatus.InProgress,
            $"Installing all extracted files from {e.ArchiveFileName}"
        );
        _webSocketServer.BroadcastToEndpointAsync("/status", message).GetAwaiter().GetResult();

        foreach (var extractedFilePath in e.ExtractedFilePaths)
        {
            _modHandlerService.HandleFileAsync(extractedFilePath).GetAwaiter().GetResult();
        }

        var completionMessage = WebSocketMessage.CreateStatus(
            taskId,
            WebSocketMessageStatus.Completed,
            $"All files from {e.ArchiveFileName} have been installed."
        );
        _webSocketServer.BroadcastToEndpointAsync("/status", completionMessage).GetAwaiter().GetResult();
    }
}

    private List<string> WaitForUserSelection(string taskId)
    {
        var tcs = new TaskCompletionSource<List<string>>();

        // Add the TaskCompletionSource to the dictionary
        _pendingSelections[taskId] = tcs;

        // Wait for the user's selection or a timeout
        if (tcs.Task.Wait(TimeSpan.FromMinutes(5)))
        {
            _pendingSelections.TryRemove(taskId, out _);
            return tcs.Task.Result;
        }
        else
        {
            _logger.Warning("Timeout waiting for user selection for task {TaskId}", taskId);
            _pendingSelections.TryRemove(taskId, out _);
            return new List<string>();
        }
    }

    private void OnWebSocketMessageReceived(object? sender, WebSocketMessageEventArgs e)
    {
        if (e.Endpoint == "/install" && e.Message.Type == WebSocketMessageType.Status && e.Message.Status == "user_selection")
        {
            if (_pendingSelections.TryGetValue(e.Message.TaskId, out var tcs))
            {
                // Deserialize the user's selection
                var selectedFiles = JsonConvert.DeserializeObject<List<string>>(e.Message.Message);
                tcs.SetResult(selectedFiles);
            }
            else
            {
                _logger.Warning("Received user selection for unknown task {TaskId}", e.Message.TaskId);
            }
        }
    }

    public void Dispose()
    {
        DisposeFileWatcher();
        _configurationService.ConfigurationChanged -= OnConfigurationChanged;

        // Unsubscribe from WebSocket messages
        _webSocketServer.MessageReceived -= OnWebSocketMessageReceived;
    }
}