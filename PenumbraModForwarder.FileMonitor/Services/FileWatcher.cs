using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using SevenZipExtractor;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.FileMonitor.Services;

public sealed class FileWatcher : IFileWatcher, IDisposable
{
    private readonly List<FileSystemWatcher> _watchers;
    private readonly ConcurrentDictionary<string, DateTime> _fileQueue;
    private readonly ConcurrentDictionary<string, int> _retryCounts;
    private readonly IFileStorage _fileStorage;
    private readonly IConfigurationService _configurationService;
    private readonly string _stateFilePath = $@"{ConfigurationConsts.ConfigurationPath}\fileQueueState.json";
    private bool _disposed;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _processingTask;
    private Timer _persistenceTimer;
    private readonly ILogger _logger;

    private static readonly Regex PreDtRegex = new(@"\[?(?i)pre[\s\-]?dt\]?", RegexOptions.Compiled);
    private const int RetryThreshold = 5;
    private const int DownloadCheckDelayMs = 1000;

    public FileWatcher(IFileStorage fileStorage, IConfigurationService configurationService)
    {
        _fileStorage = fileStorage;
        _configurationService = configurationService;

        _fileQueue = new ConcurrentDictionary<string, DateTime>();
        _retryCounts = new ConcurrentDictionary<string, int>();

        _watchers = new List<FileSystemWatcher>();
        _logger = Log.ForContext<FileWatcher>();
    }

    public event EventHandler<FileMovedEvent> FileMoved;
    public event EventHandler<FilesExtractedEventArgs> FilesExtracted;

    public async Task StartWatchingAsync(IEnumerable<string> paths)
    {
        try
        {
            await LoadStateAsync();

            foreach (var path in paths.Distinct())
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
                watcher.Renamed += OnRenamed;

                _watchers.Add(watcher);
                _logger.Information("Started watching directory: {Path}", path);
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = ProcessQueueAsync(_cancellationTokenSource.Token);

            _persistenceTimer = new Timer(_ => PersistState(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred in StartWatchingAsync.");
            throw;
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _fileQueue.TryAdd(e.FullPath, DateTime.UtcNow);
        _retryCounts[e.FullPath] = 0;
        _logger.Information("File added to queue: {FullPath}", e.FullPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _logger.Information("File renamed: from {OldFullPath} to {FullPath}", e.OldFullPath, e.FullPath);

        if (_fileQueue.TryRemove(e.OldFullPath, out var timeAdded))
        {
            _fileQueue[e.FullPath] = timeAdded;
            if (_retryCounts.TryRemove(e.OldFullPath, out var oldCount))
            {
                _retryCounts[e.FullPath] = oldCount;
            }
        }
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

                    if (!_fileStorage.Exists(filePath))
                    {
                        _logger.Warning("File does not exist, removing from queue: {FullPath}", filePath);
                        _fileQueue.TryRemove(filePath, out _);
                        _retryCounts.TryRemove(filePath, out _);
                        PersistState();
                        continue;
                    }

                    if (IsFileReady(filePath))
                    {
                        _retryCounts[filePath] = 0;
                        await ProcessFileAsync(filePath, cancellationToken);
                    }
                    else
                    {
                        var currentRetry = _retryCounts.AddOrUpdate(filePath, 1, (_, old) => old + 1);

                        // A short explanation of why we do so many attempts:
                        // We keep re-checking this file to ensure it's fully downloaded.
                        // This allows large or slow downloads to complete before we move or extract.
                        if (currentRetry < RetryThreshold)
                        {
                            if (currentRetry <= 3)
                            {
                                _logger.Information(
                                    "File not ready (attempt {Attempt}), will retry: {FullPath}",
                                    currentRetry,
                                    filePath
                                );
                            }
                            else
                            {
                                _logger.Debug(
                                    "File not ready (attempt {Attempt}), will retry: {FullPath}",
                                    currentRetry,
                                    filePath
                                );
                            }
                        }
                        else
                        {
                            if (currentRetry % 5 == 0)
                            {
                                _logger.Warning(
                                    "File is still not ready after {Attempt} attempts, continuing to wait: {FullPath}. " +
                                    "We continue re-attempting to accommodate slow file transfers and ensure completeness.",
                                    currentRetry,
                                    filePath
                                );
                            }
                            else
                            {
                                _logger.Debug(
                                    "File is still not ready after {Attempt} attempts, continuing to wait: {FullPath}",
                                    currentRetry,
                                    filePath
                                );
                            }
                        }
                    }
                }

                await Task.Delay(500, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.Information("Processing queue was canceled.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred in ProcessQueueAsync.");
        }
    }

    private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!_fileStorage.Exists(filePath))
        {
            _fileQueue.TryRemove(filePath, out _);
            _retryCounts.TryRemove(filePath, out _);
            PersistState();
            _logger.Warning("File no longer exists: {FullPath}", filePath);
            return;
        }

        if (!IsFileReady(filePath))
        {
            _logger.Information("File is not ready for processing: {FullPath}", filePath);
            return;
        }

        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

        if (FileExtensionsConsts.ModFileTypes.Contains(extension))
        {
            var movedFilePath = MoveFile(filePath);
            _fileQueue.TryRemove(filePath, out _);
            _retryCounts.TryRemove(filePath, out _);
            PersistState();

            var fileName = Path.GetFileName(movedFilePath);
            FileMoved?.Invoke(this, new FileMovedEvent(fileName, movedFilePath, Path.GetFileNameWithoutExtension(movedFilePath)));
        }
        else if (FileExtensionsConsts.ArchiveFileTypes.Contains(extension))
        {
            if (await ArchiveContainsModFileAsync(filePath, cancellationToken))
            {
                _logger.Information("Archive {FilePath} contains one or more mod files; proceeding with move and extraction.", filePath);

                var movedFilePath = MoveFile(filePath);
                _fileQueue.TryRemove(filePath, out _);
                _retryCounts.TryRemove(filePath, out _);
                PersistState();

                await ProcessArchiveFileAsync(movedFilePath, cancellationToken);
            }
            else
            {
                _logger.Information("Archive {FilePath} does not contain any mod files; leaving file as-is.", filePath);

                _fileQueue.TryRemove(filePath, out _);
                _retryCounts.TryRemove(filePath, out _);
                PersistState();
            }
        }
        else
        {
            _logger.Warning("Unhandled file type: {FullPath}", filePath);
            _fileQueue.TryRemove(filePath, out _);
            _retryCounts.TryRemove(filePath, out _);
            PersistState();
        }
    }

    private async Task ProcessArchiveFileAsync(string archivePath, CancellationToken cancellationToken)
    {
        try
        {
            var extractedFiles = new List<string>();

            using (var archiveFile = new ArchiveFile(archivePath))
            {
                var skipPreDt = (bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.SkipPreDt);
                var baseDirectory = (string)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.ModFolderPath);

                var modEntries = archiveFile.Entries.Where(entry =>
                {
                    var entryExtension = Path.GetExtension(entry.FileName)?.ToLowerInvariant();
                    if (!FileExtensionsConsts.ModFileTypes.Contains(entryExtension))
                    {
                        return false;
                    }

                    if (skipPreDt)
                    {
                        var entryPath = entry.FileName;
                        var directories = entryPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

                        if (directories.Any(dir => PreDtRegex.IsMatch(dir)))
                        {
                            _logger.Information("Skipping file in Pre-Dt folder: {FileName}", entry.FileName);
                            return false;
                        }
                    }
                    return true;
                }).ToList();

                if (modEntries.Any())
                {
                    _logger.Information("Archive contains mod files: {ArchiveFileName}", Path.GetFileName(archivePath));

                    foreach (var entry in modEntries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var destinationPath = Path.Combine(baseDirectory, entry.FileName);
                        var destinationDir = Path.GetDirectoryName(destinationPath);
                        _fileStorage.CreateDirectory(destinationDir);

                        _logger.Information("Extracting mod file: {FileName} to {DestinationPath}", entry.FileName, destinationPath);

                        entry.Extract(destinationPath);
                        extractedFiles.Add(destinationPath);

                        _logger.Information("Extraction complete for: {FileName}", entry.FileName);
                    }
                }
                else
                {
                    _logger.Information("No mod files found in archive: {ArchiveFileName}", Path.GetFileName(archivePath));
                }
            }

            await Task.Delay(100, cancellationToken);

            if (extractedFiles.Any())
            {
                var archiveFileName = Path.GetFileName(archivePath);
                FilesExtracted?.Invoke(this, new FilesExtractedEventArgs(archiveFileName, extractedFiles));

                var shouldDelete = (bool)_configurationService.ReturnConfigValue(config => config.BackgroundWorker.AutoDelete);
                if (shouldDelete)
                {
                    DeleteFileWithRetry(archivePath);
                    _logger.Information("Deleted archive after extraction: {ArchiveFileName}", Path.GetFileName(archivePath));
                }
            }
            else
            {
                _logger.Information("No mod files found; leaving file intact: {ArchiveFileName}", Path.GetFileName(archivePath));
            }
        }
        catch (SevenZipException ex) when (ex.Message.Contains("not a known archive type"))
        {
            _logger.Warning("File is not recognized as a valid archive: {ArchiveFilePath}", archivePath);
            DeleteFileWithRetry(archivePath);
            _logger.Information("Deleted invalid archive: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Processing of archive was canceled: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing archive: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
    }

    private string MoveFile(string filePath)
    {
        var modPath = (string)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.ModFolderPath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var destinationFolder = Path.Combine(modPath, fileNameWithoutExtension);
        _fileStorage.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
        _fileStorage.CopyFile(filePath, destinationPath, overwrite: true);

        DeleteFileWithRetry(filePath);
        _logger.Information("File moved: {SourcePath} to {DestinationPath}", filePath, destinationPath);

        return destinationPath;
    }
    
    private async Task<bool> ArchiveContainsModFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            using var archiveFile = new ArchiveFile(filePath);
            var skipPreDt = (bool)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.SkipPreDt);

            var hasModFile = archiveFile.Entries.Any(entry =>
            {
                var entryExtension = Path.GetExtension(entry.FileName)?.ToLowerInvariant();
                if (!FileExtensionsConsts.ModFileTypes.Contains(entryExtension))
                {
                    return false;
                }

                if (skipPreDt && entry.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(dir => PreDtRegex.IsMatch(dir)))
                {
                    return false;
                }

                return true;
            });

            return hasModFile;
        }
        catch (SevenZipException ex) when (ex.Message.Contains("not a known archive type"))
        {
            _logger.Warning("File {FilePath} is not recognized as a valid archive.", filePath);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Archive check was canceled for {FilePath}.", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for mod files in {FilePath}.", filePath);
            return false;
        }
        finally
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    private void DeleteFileWithRetry(string filePath, int maxAttempts = 3, int delayMilliseconds = 500)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _fileStorage.Delete(filePath);
                _logger.Information("Deleted file on attempt {Attempt}: {FilePath}", attempt, filePath);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                _logger.Warning(
                    "Attempt {Attempt} to delete file failed: {FilePath}. Retrying in {Delay}ms...",
                    attempt,
                    filePath,
                    delayMilliseconds
                );
                Thread.Sleep(delayMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete file: {FilePath}", filePath);
                throw;
            }
        }

        try
        {
            _fileStorage.Delete(filePath);
            _logger.Information("Deleted file on final attempt: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete file after multiple attempts: {FilePath}", filePath);
            throw;
        }
    }

    private bool IsFileReady(string filePath)
    {
        if (!IsFileFullyDownloaded(filePath))
        {
            return false;
        }

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

    private bool IsFileFullyDownloaded(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(filePath);
                var searchPattern = fileNameNoExtension + ".*.part";

                var partFiles = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
                if (partFiles.Length > 0)
                {
                    _logger.Debug(
                        "Detected one or more part files related to {FilePath}. The file is still downloading.",
                        filePath
                    );
                    return false;
                }
            }
            else
            {
                _logger.Warning("Could not determine directory for {FilePath}, skipped part-file check.", filePath);
            }

            const int maxChecks = 3;
            long lastSize = -1;

            for (int i = 0; i < maxChecks; i++)
            {
                var fileInfo = new FileInfo(filePath);
                var currentSize = fileInfo.Length;

                if (lastSize == currentSize && currentSize != 0)
                {
                    return true;
                }

                lastSize = currentSize;
                Thread.Sleep(DownloadCheckDelayMs);
            }

            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error checking if file is fully downloaded: {FilePath}", filePath);
            return false;
        }
    }

    private void PersistState()
    {
        try
        {
            var serializedQueue = JsonConvert.SerializeObject(_fileQueue);
            _fileStorage.Write(_stateFilePath, serializedQueue);
            _logger.Information("File queue state persisted.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to persist file queue state.");
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
                    foreach (var kvp in deserializedQueue)
                    {
                        if (_fileStorage.Exists(kvp.Key))
                        {
                            _fileQueue.TryAdd(kvp.Key, kvp.Value);
                            _retryCounts[kvp.Key] = 0;
                        }
                        else
                        {
                            _logger.Warning("File from saved state no longer exists: {FullPath}", kvp.Key);
                        }
                    }
                }

                _logger.Information("File queue state loaded.");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load file queue state.");
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
                _logger.Error(ex, "An error occurred while persisting state during disposal.");
            }

            _persistenceTimer?.Dispose();

            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();
            _logger.Information("All FileWatchers disposed.");

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