using System.Collections.Concurrent;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using SevenZipExtractor;

namespace PenumbraModForwarder.FileMonitor.Services;

public class FileWatcher : IFileWatcher, IDisposable
{
    private readonly List<FileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, DateTime> _fileQueue;
    private readonly IFileStorage _fileStorage;
    private readonly string _destDirectory = ConfigurationConsts.ModsPath;
    private readonly string _stateFilePath = "fileQueueState.json";
    private bool _disposed;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _processingTask;

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

                if (FileExtensionsConsts.ArchiveFileTypes.Contains(extension))
                {
                    await ProcessArchiveFileAsync(filePath, cancellationToken);
                }
                else if (FileExtensionsConsts.ModFileTypes.Contains(extension))
                {
                    MoveFile(filePath);
                    _fileQueue.TryRemove(filePath, out _);
                }
                else
                {
                    Log.Warning("Unhandled file type: {FullPath}", filePath);
                    _fileQueue.TryRemove(filePath, out _);
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
                    MoveFile(archivePath);
                    _fileQueue.TryRemove(archivePath, out _);
                    Log.Information("Archive moved: {ArchivePath}", archivePath);
                }
                else
                {
                    Log.Information("Archive does not contain mod files: {ArchivePath}", archivePath);
                    _fileQueue.TryRemove(archivePath, out _);
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

    private void MoveFile(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var destinationFolder = Path.Combine(_destDirectory, fileNameWithoutExtension);
        _fileStorage.CreateDirectory(destinationFolder);
        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
        File.Move(filePath, destinationPath, overwrite: true);
        _fileQueue.TryRemove(filePath, out _);
        FileMoved?.Invoke(this, new FileMovedEvent(filePath, destinationPath, fileNameWithoutExtension));
        Log.Information("File moved: {SourcePath} to {DestinationPath}", filePath, destinationPath);
    }

    private bool IsFileReady(string filePath)
    {
        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public async Task PersistStateAsync()
    {
        try
        {
            var serializedQueue = JsonConvert.SerializeObject(_fileQueue);
            await File.WriteAllTextAsync(_stateFilePath, serializedQueue);
            Log.Information("File queue state persisted.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to persist file queue state.");
        }
    }

    public async Task LoadStateAsync()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var serializedQueue = await File.ReadAllTextAsync(_stateFilePath);
                var deserializedQueue = JsonConvert.DeserializeObject<ConcurrentDictionary<string, DateTime>>(serializedQueue);
                if (deserializedQueue != null)
                {
                    foreach (var item in deserializedQueue)
                    {
                        _fileQueue.TryAdd(item.Key, item.Value);
                    }
                }
                Log.Information("File queue state loaded.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load file queue state.");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
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