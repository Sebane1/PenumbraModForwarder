using System.Collections.Concurrent;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using SevenZipExtractor;

namespace PenumbraModForwarder.FileMonitor.Services;

public sealed class FileWatcher : IFileWatcher, IDisposable
{
    private readonly List<FileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, DateTime> _fileQueue;
    private readonly IFileStorage _fileStorage;
    private readonly string _destDirectory = ConfigurationConsts.ModsPath;
    private readonly string _stateFilePath = $@"{ConfigurationConsts.ConfigurationPath}\fileQueueState.json";
    private bool _disposed;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _processingTask;
    private Timer _persistenceTimer;

    public FileWatcher(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
        _fileQueue = new ConcurrentDictionary<string, DateTime>();
        _watchers = new List<FileSystemWatcher>();
    }

    public event EventHandler<FileMovedEvent> FileMoved;

    public async Task StartWatchingAsync(IEnumerable<string> paths)
    {
        try
        {
            await LoadStateAsync();

            foreach (var path in paths)
            {
                var watcher = new FileSystemWatcher(path)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    EnableRaisingEvents = true,
                };

                foreach (var extension in FileExtensionsConsts.AllowedExtensions)
                {
                    watcher.Filters.Add($"*{extension}");
                }

                watcher.Created += OnCreated;
                _watchers.Add(watcher);
                Log.Information("Started watching directory: {Path}", path);
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = ProcessQueueAsync(_cancellationTokenSource.Token);

            // Initialize the persistence timer to save the queue state every minute
            _persistenceTimer = new Timer(_ => PersistState(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred in StartWatchingAsync.");
            throw;
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _fileQueue.TryAdd(e.FullPath, DateTime.UtcNow);
        Log.Information("File added to queue: {FullPath}", e.FullPath);
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var filesToProcess = _fileQueue.Keys.ToList();

                foreach (var filePath in filesToProcess)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (IsFileReady(filePath))
                    {
                        await ProcessFileAsync(filePath, cancellationToken);
                    }
                    else if (DateTime.UtcNow - _fileQueue[filePath] > TimeSpan.FromSeconds(5))
                    {
                        Log.Information("File is not ready for processing, will retry: {FullPath}", filePath);
                    }
                }

                await Task.Delay(500, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Log.Information("Processing queue was canceled.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred in ProcessQueueAsync.");
        }
    }

    private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (_fileStorage.Exists(filePath))
        {
            if (IsFileReady(filePath))
            {
                var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

                if (FileExtensionsConsts.ArchiveFileTypes.Contains(extension) ||
                    FileExtensionsConsts.ModFileTypes.Contains(extension))
                {
                    var movedFilePath = MoveFile(filePath);
                    _fileQueue.TryRemove(filePath, out _);
                    PersistState();

                    if (FileExtensionsConsts.ArchiveFileTypes.Contains(extension))
                    {
                        await ProcessArchiveFileAsync(movedFilePath, cancellationToken);
                    }
                }
                else
                {
                    Log.Warning("Unhandled file type: {FullPath}", filePath);
                    _fileQueue.TryRemove(filePath, out _);
                    PersistState();
                }
            }
            else
            {
                Log.Information("File is not ready for processing: {FullPath}", filePath);
            }
        }
        else
        {
            _fileQueue.TryRemove(filePath, out _);
            PersistState();
            Log.Warning("File no longer exists: {FullPath}", filePath);
        }
    }

    private async Task ProcessArchiveFileAsync(string archivePath, CancellationToken cancellationToken)
    {
        try
        {
            using (var archiveFile = new ArchiveFile(archivePath))
            {
                bool containsModFile = archiveFile.Entries.Any(entry =>
                {
                    var entryExtension = Path.GetExtension(entry?.FileName)?.ToLowerInvariant();
                    return FileExtensionsConsts.ModFileTypes.Contains(entryExtension);
                });

                if (containsModFile)
                {
                    Log.Information("Archive contains mod files: {ArchivePath}", archivePath);
                    // Perform additional processing if necessary
                }
                else
                {
                    Log.Information("Archive does not contain mod files: {ArchivePath}", archivePath);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Processing of archive was canceled: {ArchivePath}", archivePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing archive: {ArchivePath}", archivePath);
        }

        await Task.CompletedTask;
    }

    private string MoveFile(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var destinationFolder = Path.Combine(_destDirectory, fileNameWithoutExtension);
        _fileStorage.CreateDirectory(destinationFolder);
        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));

        // Copy and delete to simulate move
        _fileStorage.CopyFile(filePath, destinationPath, overwrite: true);

        // Retry deletion in case of transient errors
        DeleteFileWithRetry(filePath);

        FileMoved?.Invoke(this, new FileMovedEvent(filePath, destinationPath, fileNameWithoutExtension));
        Log.Information("File moved: {SourcePath} to {DestinationPath}", filePath, destinationPath);

        return destinationPath;
    }

    private void DeleteFileWithRetry(string filePath, int maxAttempts = 3, int delayMilliseconds = 500)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _fileStorage.Delete(filePath);
                return;
            }
            catch (IOException ex) when (attempt < maxAttempts)
            {
                Log.Warning("Attempt {Attempt} to delete file failed: {FilePath}. Retrying in {Delay}ms...", attempt, filePath, delayMilliseconds);
                Thread.Sleep(delayMilliseconds);
            }
        }

        // Final attempt
        _fileStorage.Delete(filePath);
    }

    private bool IsFileReady(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private void PersistState()
    {
        try
        {
            var serializedQueue = JsonConvert.SerializeObject(_fileQueue);
            _fileStorage.Write(_stateFilePath, serializedQueue);
            Log.Information("File queue state persisted.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to persist file queue state.");
        }
    }

    private async Task LoadStateAsync()
    {
        try
        {
            if (_fileStorage.Exists(_stateFilePath))
            {
                var serializedQueue = _fileStorage.Read(_stateFilePath);
                var deserializedQueue = JsonConvert.DeserializeObject<ConcurrentDictionary<string, DateTime>>(serializedQueue);
                if (deserializedQueue != null)
                {
                    foreach (var item in deserializedQueue)
                    {
                        if (_fileStorage.Exists(item.Key))
                        {
                            _fileQueue.TryAdd(item.Key, item.Value);
                        }
                        else
                        {
                            Log.Warning("File from saved state no longer exists: {FullPath}", item.Key);
                        }
                    }
                }
                Log.Information("File queue state loaded.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load file queue state.");
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            try
            {
                PersistState();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while persisting state during disposal.");
            }

            _persistenceTimer?.Dispose();

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();
            Log.Information("All FileWatchers disposed.");

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_processingTask != null)
            {
                try
                {
                    _processingTask.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle(ex => ex is TaskCanceledException);
                }
                _processingTask = null;
            }
        }
        _disposed = true;
    }
}