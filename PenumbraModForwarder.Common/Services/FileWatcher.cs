using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Timer = System.Timers.Timer;

namespace PenumbraModForwarder.Common.Services
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly ILogger<FileWatcher> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IFileHandlerService _fileHandlerService;
        private readonly IErrorWindowService _errorWindowService;
        private FileSystemWatcher _watcher;

        private readonly Timer _debounceTimer;
        private readonly Timer _retryTimer;
        private readonly Timer _clearProcessedFilesTimer;

        private readonly ConcurrentQueue<string> _changeQueue;
        private readonly ConcurrentQueue<string> _retryQueue;
        private readonly ConcurrentDictionary<string, bool> _processingFiles;
        private readonly ConcurrentDictionary<string, DateTime> _trackedFiles;
        private readonly ConcurrentDictionary<string, FileDownloadInfo> _downloadProgress;
        private readonly HashSet<string> _processedFiles;
        
        private readonly TimeSpan _initialDelay = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _stabilityCheckInterval = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _archiveStabilityDelay = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _eventCooldown = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _maxWaitTime = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _progressTimeout = TimeSpan.FromMinutes(5);
        private readonly int _requiredStabilityChecks = 2;
        private readonly long _minSizeProgress = 1024;

        private readonly string[] _archiveExtensions = { ".zip", ".rar", ".7z" };
        private readonly string[] _modExtensions = { ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };

        private int _ongoingProcessingCount;
        private readonly object _lock = new object();

        private CancellationTokenSource _cancellationTokenSource;

        public FileWatcher(ILogger<FileWatcher> logger, IConfigurationService configurationService,
            IFileHandlerService fileHandlerService, IErrorWindowService errorWindowService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _fileHandlerService = fileHandlerService;
            _errorWindowService = errorWindowService;

            _changeQueue = new ConcurrentQueue<string>();
            _retryQueue = new ConcurrentQueue<string>();
            _processingFiles = new ConcurrentDictionary<string, bool>();
            _trackedFiles = new ConcurrentDictionary<string, DateTime>();
            _downloadProgress = new ConcurrentDictionary<string, FileDownloadInfo>();
            _processedFiles = new HashSet<string>();

            _debounceTimer = CreateDebounceTimer();
            _retryTimer = CreateRetryTimer();
            _clearProcessedFilesTimer = CreateClearProcessedFilesTimer();

            _cancellationTokenSource = new CancellationTokenSource();

            if (_configurationService.GetConfigValue(config => config.AutoLoad))
            {
                InitializeWatcher();
            }

            _configurationService.ConfigChanged += OnConfigChange;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _debounceTimer?.Dispose();
            _retryTimer?.Dispose();
            _clearProcessedFilesTimer?.Dispose();
            _watcher?.Dispose();
            _configurationService.ConfigChanged -= OnConfigChange;
        }

        public void ClearQueues()
        {
            lock (_lock)
            {
                _logger.LogInformation("Clearing all queues and tracked files.");
                while (_changeQueue.TryDequeue(out _)) { }
                while (_retryQueue.TryDequeue(out _)) { }
                _trackedFiles.Clear();
                _processedFiles.Clear();
                _processingFiles.Clear();
            }
        }

        private Timer CreateDebounceTimer()
        {
            var timer = new Timer(_debounceInterval.TotalMilliseconds) { AutoReset = false };
            timer.Elapsed += (sender, args) => ProcessFileChanges();
            return timer;
        }

        private Timer CreateRetryTimer()
        {
            var timer = new Timer(_retryInterval.TotalMilliseconds) { AutoReset = true };
            timer.Elapsed += (sender, args) => ProcessRetryQueue();
            timer.Start();
            return timer;
        }

        private Timer CreateClearProcessedFilesTimer()
        {
            var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds) { AutoReset = true };
            timer.Elapsed += (sender, args) => TryClearProcessedFiles();
            timer.Start();
            return timer;
        }

        private void OnConfigChange(object sender, EventArgs e)
        {
            _logger.LogInformation("Config changed, reinitializing watcher.");
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            _logger.LogInformation("Initializing file system watcher.");

            if (_watcher != null)
            {
                _watcher.Created -= OnFileCreated;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Dispose();
                _logger.LogDebug("Previous watcher disposed.");
            }
            
            var directory = _configurationService.GetConfigValue(config => config.DownloadPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                _logger.LogWarning($"Directory '{directory}' does not exist or is invalid.");
                return;
            }

            _watcher = new FileSystemWatcher
            {
                Path = directory,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = false
            };

            _watcher.Created += OnFileCreated;
            _watcher.Renamed += OnFileRenamed;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation($"Watcher set up and watching directory: {directory}");
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            _logger.LogInformation($"File renamed: {e.FullPath}");
            if (!_configurationService.GetConfigValue(o => o.AutoLoad)) return;
            // Check if this file is already being tracked before enqueueing
            if (!_trackedFiles.ContainsKey(e.FullPath) && !_processingFiles.ContainsKey(e.FullPath))
            {
                EnqueueFileEvent(e.FullPath);
            }
            else
            {
                _logger.LogDebug($"Skipping renamed event for already tracked file: {e.FullPath}");
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"File created: {e.FullPath}");
            if (!_configurationService.GetConfigValue(o => o.AutoLoad)) return;
            // Check if this file is already being tracked before enqueueing
            if (!_trackedFiles.ContainsKey(e.FullPath) && !_processingFiles.ContainsKey(e.FullPath))
            {
                EnqueueFileEvent(e.FullPath);
            }
            else
            {
                _logger.LogDebug($"Skipping created event for already tracked file: {e.FullPath}");
            }
        }

        private async void EnqueueFileEvent(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath).ToLower();
            // Combine both extension arrays for initial check
            var validExtensions = _archiveExtensions.Concat(_modExtensions);
            if (!validExtensions.Contains(fileExtension))
            {
                _logger.LogInformation($"Ignored file: {filePath}, unsupported extension.");
                return;
            }

            // Initial delay - slightly longer for archives
            if (_archiveExtensions.Contains(fileExtension))
            {
                _logger.LogDebug($"Archive file detected, using extended initial delay: {filePath}");
                await Task.Delay(_archiveStabilityDelay);
            }
            else
            {
                await Task.Delay(_initialDelay);
            }

            lock (_lock)
            {
                var now = DateTime.UtcNow;

                // Early return if file is already being tracked or processed
                if (_trackedFiles.ContainsKey(filePath) || 
                    _processingFiles.ContainsKey(filePath) || 
                    _processedFiles.Contains(filePath))
                {
                    _logger.LogDebug($"File '{filePath}' is already being tracked or processed.");
                    return;
                }

                if (_trackedFiles.TryGetValue(filePath, out var lastProcessed))
                {
                    if (now - lastProcessed < _eventCooldown)
                    {
                        _logger.LogInformation($"Ignoring file event for '{filePath}' due to cooldown.");
                        return;
                    }
                }

                // Initialize download tracking only if not already tracked
                if (!_downloadProgress.ContainsKey(filePath))
                {
                    _downloadProgress[filePath] = new FileDownloadInfo
                    {
                        LastSize = 0,
                        LastProgressTime = now,
                        StartTime = now,
                        StabilityCount = 0
                    };

                    _retryQueue.Enqueue(filePath);
                    _logger.LogInformation($"Queued {(_archiveExtensions.Contains(fileExtension) ? "archive" : "mod")} file for stability check: {filePath}");
                }
            }
        }
        
        private async Task<bool> WaitForFileStability(string filePath, CancellationToken cancellationToken)
        {
            if (!_downloadProgress.TryGetValue(filePath, out var downloadInfo))
            {
                _logger.LogWarning($"No download info found for file: {filePath}");
                return false;
            }

            try
            {
                var lastSize = 0L;
                var stableCount = 0;
                var fileInfo = new FileInfo(filePath);
                var isArchive = _archiveExtensions.Contains(Path.GetExtension(filePath).ToLower());

                while (!cancellationToken.IsCancellationRequested)
                {
                    var now = DateTime.UtcNow;
                    
                    if (now - downloadInfo.StartTime > _maxWaitTime)
                    {
                        _logger.LogWarning($"Download exceeded maximum wait time for file: {filePath}");
                        return false;
                    }

                    if (!fileInfo.Exists)
                    {
                        _logger.LogWarning($"File no longer exists: {filePath}");
                        return false;
                    }

                    // Refresh FileInfo to get current size
                    fileInfo.Refresh();
                    var currentSize = fileInfo.Length;

                    if (currentSize > 0)
                    {
                        if (currentSize > downloadInfo.LastSize + _minSizeProgress)
                        {
                            // Progress detected
                            downloadInfo.LastSize = currentSize;
                            downloadInfo.LastProgressTime = now;
                            stableCount = 0;
                            _logger.LogDebug($"Download progress detected for {filePath}: {currentSize} bytes");
                        }
                        else if (now - downloadInfo.LastProgressTime > _progressTimeout)
                        {
                            _logger.LogWarning($"Download progress timeout for file: {filePath}");
                            return false;
                        }

                        // Check if file size is stable
                        if (currentSize == lastSize)
                        {
                            if (isArchive)
                            {
                                // For archives, we need additional checks
                                if (await IsArchiveAccessible(filePath))
                                {
                                    stableCount++;
                                    _logger.LogDebug($"Archive stability check {stableCount}/{_requiredStabilityChecks} for {filePath}");
                                }
                                else
                                {
                                    stableCount = 0;
                                }
                            }
                            else if (IsFileReady(filePath))
                            {
                                stableCount++;
                                _logger.LogDebug($"Stability check {stableCount}/{_requiredStabilityChecks} for {filePath}");
                            }
                            
                            if (stableCount >= _requiredStabilityChecks)
                            {
                                // For archives, add an additional delay after stability is confirmed
                                if (isArchive)
                                {
                                    _logger.LogDebug($"Adding additional stability delay for archive: {filePath}");
                                    await Task.Delay(_archiveStabilityDelay, cancellationToken);
                                    
                                    // One final check after the delay
                                    if (!await IsArchiveAccessible(filePath))
                                    {
                                        _logger.LogWarning($"Archive failed final accessibility check: {filePath}");
                                        return false;
                                    }
                                }

                                _logger.LogInformation($"File {filePath} has stabilized at {currentSize} bytes");
                                return true;
                            }
                        }
                        else
                        {
                            stableCount = 0;
                        }

                        lastSize = currentSize;
                    }

                    await Task.Delay(_stabilityCheckInterval, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking file stability: {filePath}");
                return false;
            }
            finally
            {
                _downloadProgress.TryRemove(filePath, out _);
            }

            return false;
        }
        
        private async Task<bool> IsArchiveAccessible(string filePath)
        {
            try
            {
                // First check if we can open the file
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // Read the first few bytes to verify file access
                    var buffer = new byte[4096];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Archive {filePath} not yet accessible: {ex.Message}");
                return false;
            }
        }

        private void ProcessFileChanges()
        {
            while (_changeQueue.TryDequeue(out var file))
            {
                lock (_lock)
                {
                    if (_processedFiles.Contains(file))
                    {
                        _logger.LogDebug($"File '{file}' has already been processed.");
                        _processingFiles.TryRemove(file, out _);
                        continue;
                    }

                    _ongoingProcessingCount++;
                }

                ProcessFileAsync(file, _cancellationTokenSource.Token);
            }
        }

        private async void ProcessFileAsync(string file, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        _fileHandlerService.HandleFile(file);
                        _processedFiles.Add(file);
                        _logger.LogInformation($"Added file '{file}' to processed files collection.");
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process file: {file}");
                _errorWindowService.ShowError($"Failed to process file: {file}");
            }
            finally
            {
                lock (_lock)
                {
                    _processingFiles.TryRemove(file, out _);
                    _ongoingProcessingCount--;
                }

                _logger.LogInformation($"File processing completed: {file}");
            }
        }

        private void ProcessRetryQueue()
        {
            _logger.LogDebug("Processing retry queue.");
            var retryQueueCopy = new List<string>();

            while (_retryQueue.TryDequeue(out var filePath))
            {
                retryQueueCopy.Add(filePath);
            }

            foreach (var filePath in retryQueueCopy)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        if (await WaitForFileStability(filePath, _cancellationTokenSource.Token))
                        {
                            lock (_lock)
                            {
                                if (!_processingFiles.ContainsKey(filePath) && !_processedFiles.Contains(filePath))
                                {
                                    _trackedFiles[filePath] = DateTime.UtcNow;
                                    _processingFiles.TryAdd(filePath, true);
                                    _changeQueue.Enqueue(filePath);
                                    RestartDebounceTimer();
                                    _logger.LogInformation($"File {filePath} is stable and queued for processing");
                                }
                            }
                        }
                        else
                        {
                            _retryQueue.Enqueue(filePath);
                            _logger.LogInformation($"File {filePath} is not stable, re-queuing");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing file {filePath}");
                        _retryQueue.Enqueue(filePath);
                    }
                });
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Unexpected error checking file readiness: {filePath}");
                return false;
            }
        }
        
        private void RestartDebounceTimer()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void TryClearProcessedFiles()
        {
            _logger.LogDebug("Trying to clear processed files collection.");
            lock (_lock)
            {
                if (_ongoingProcessingCount == 0)
                {
                    _logger.LogInformation("Clearing processed files collection and tracked files.");
                    _processedFiles.Clear();
                    _trackedFiles.Clear();
                    _fileHandlerService.CleanUpTempFiles();
                }
            }
        }
    }
}