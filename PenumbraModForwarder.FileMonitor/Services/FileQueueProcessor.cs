using System.Collections.Concurrent;
using Newtonsoft.Json;
using NLog;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;

namespace PenumbraModForwarder.FileMonitor.Services;

public sealed class FileQueueProcessor : IFileQueueProcessor
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly ConcurrentDictionary<string, DateTime> _fileQueue;
    private readonly ConcurrentDictionary<string, int> _retryCounts;
    private readonly IFileStorage _fileStorage;
    private readonly IConfigurationService _configurationService;
    private readonly IFileProcessor _fileProcessor;

    private CancellationTokenSource _cancellationTokenSource;
    private Task _processingTask;
    private System.Threading.Timer _persistenceTimer;
    private readonly string _stateFilePath;

    public event EventHandler<FileMovedEvent> FileMoved;
    public event EventHandler<FilesExtractedEventArgs> FilesExtracted;

    public FileQueueProcessor(
        IFileStorage fileStorage,
        IConfigurationService configurationService,
        IFileProcessor fileProcessor)
    {
        _fileQueue = new ConcurrentDictionary<string, DateTime>();
        _retryCounts = new ConcurrentDictionary<string, int>();

        _fileStorage = fileStorage;
        _configurationService = configurationService;
        _fileProcessor = fileProcessor;

        _stateFilePath = Path.Combine(ConfigurationConsts.ConfigurationPath, "fileQueueState.json");
    }

    public void EnqueueFile(string fullPath)
    {
        _fileQueue[fullPath] = DateTime.UtcNow;
        _retryCounts[fullPath] = 0;
    }

    public void RenameFileInQueue(string oldPath, string newPath)
    {
        if (_fileQueue.TryRemove(oldPath, out var timeAdded))
        {
            _fileQueue[newPath] = timeAdded;

            if (_retryCounts.TryRemove(oldPath, out var oldCount))
            {
                _retryCounts[newPath] = oldCount;
            }
        }
        else
        {
            var extension = Path.GetExtension(newPath)?.ToLowerInvariant();
            if (FileExtensionsConsts.AllowedExtensions.Contains(extension))
            {
                EnqueueFile(newPath);
                _logger.Info("File added to queue after rename (unrecognized old path): {FullPath}", newPath);
            }
        }
    }

    public async Task LoadStateAsync()
    {
        try
        {
            if (_fileStorage.Exists(_stateFilePath))
            {
                var serializedQueue = _fileStorage.Read(_stateFilePath);
                var deserializedQueue = JsonConvert.DeserializeObject<ConcurrentDictionary<string, DateTime>>(serializedQueue);
                if (deserializedQueue != null)
                {
                    foreach (var kvp in deserializedQueue)
                    {
                        if (_fileStorage.Exists(kvp.Key))
                        {
                            _fileQueue[kvp.Key] = kvp.Value;
                            _retryCounts[kvp.Key] = 0;
                        }
                        else
                        {
                            _logger.Warn("File from state no longer exists: {FullPath}", kvp.Key);
                        }
                    }
                }
                _logger.Info("File queue state loaded successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load file queue state.");
        }

        await Task.CompletedTask;
    }

    public void PersistState()
    {
        try
        {
            var serializedQueue = JsonConvert.SerializeObject(_fileQueue);
            _fileStorage.Write(_stateFilePath, serializedQueue);
            _logger.Info("File queue state persisted.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to persist file queue state.");
        }
    }

    public void StartProcessing()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _processingTask = ProcessQueueAsync(_cancellationTokenSource.Token);

        // Save state every minute
        _persistenceTimer = new System.Threading.Timer(
            _ => PersistState(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1)
        );
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var filesToProcess = _fileQueue.Keys.ToList();
                var hasChanges = false;

                foreach (var filePath in filesToProcess)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (!_fileStorage.Exists(filePath))
                    {
                        if (_fileQueue.TryRemove(filePath, out _) | _retryCounts.TryRemove(filePath, out _))
                        {
                            _logger.Warn("File not found, removing from queue: {FullPath}", filePath);
                            hasChanges = true;
                        }
                        continue;
                    }

                    if (_fileProcessor.IsFileReady(filePath))
                    {
                        _retryCounts[filePath] = 0;

                        await _fileProcessor.ProcessFileAsync(
                            filePath,
                            cancellationToken,
                            OnFileMoved,
                            OnFilesExtracted
                        );

                        if (_fileQueue.TryRemove(filePath, out _) | _retryCounts.TryRemove(filePath, out _))
                        {
                            hasChanges = true;
                        }
                    }
                    else
                    {
                        var currentRetry = _retryCounts.AddOrUpdate(filePath, 1, (_, oldValue) => oldValue + 1);

                        if (currentRetry < 5)
                        {
                            // Log at Info for first three attempts, then Debug
                            if (currentRetry <= 3)
                            {
                                _logger.Info("File not ready (attempt {Attempt}), requeue: {FullPath}", currentRetry, filePath);
                            }
                            else
                            {
                                _logger.Debug("File not ready (attempt {Attempt}), requeue: {FullPath}", currentRetry, filePath);
                            }
                        }
                        else
                        {
                            // For attempts >= 5, log at Warning only every 5 attempts
                            if (currentRetry % 5 == 0)
                            {
                                _logger.Warn("File not ready after {Attempt} attempts: {FullPath}", currentRetry, filePath);
                            }
                            else
                            {
                                _logger.Debug("File not ready after {Attempt} attempts: {FullPath}", currentRetry, filePath);
                            }
                        }
                    }
                }

                if (hasChanges)
                {
                    PersistState();
                }

                await Task.Delay(500, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.Info("Processing queue was canceled.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing queue.");
        }
    }

    private void OnFileMoved(object sender, FileMovedEvent e) => FileMoved?.Invoke(this, e);
    private void OnFilesExtracted(object sender, FilesExtractedEventArgs e) => FilesExtracted?.Invoke(this, e);
}