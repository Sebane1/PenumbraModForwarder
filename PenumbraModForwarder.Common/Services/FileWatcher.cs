using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using Timer = System.Timers.Timer;

namespace PenumbraModForwarder.Common.Services
{
    public class FileWatcher : IFileWatcher
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
        private readonly HashSet<string> _processedFiles;

        private readonly TimeSpan _debounceInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _eventCooldown = TimeSpan.FromSeconds(5);
        private readonly string[] _allowedExtensions = { ".zip", ".rar", ".7z", ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };

        private int _ongoingProcessingCount;
        private readonly object _lock = new object();

        private CancellationTokenSource _cancellationTokenSource;

        public FileWatcher(ILogger<FileWatcher> logger, IConfigurationService configurationService, IFileHandlerService fileHandlerService, IErrorWindowService errorWindowService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationService = configurationService;
            _fileHandlerService = fileHandlerService;
            _errorWindowService = errorWindowService;

            _changeQueue = new ConcurrentQueue<string>();
            _retryQueue = new ConcurrentQueue<string>();
            _processingFiles = new ConcurrentDictionary<string, bool>();
            _trackedFiles = new ConcurrentDictionary<string, DateTime>();
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

        public void ClearQueues()
        {
            lock (_lock)
            {
                _logger.LogInformation("Clearing all queues and tracked files.");
                while (_changeQueue.TryDequeue(out _)) { }
                while (_retryQueue.TryDequeue(out _)) { }
                _trackedFiles.Clear();
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
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _watcher.Created += OnFileCreated;
            _watcher.Renamed += OnFileRenamed;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation($"Watcher set up and watching directory: {directory}");
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            _logger.LogInformation($"File renamed: {e.FullPath}");
            if (_configurationService.GetConfigValue(o => o.AutoLoad))
            {
                EnqueueFileEvent(e.FullPath);
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"File created: {e.FullPath}");
            if (_configurationService.GetConfigValue(o => o.AutoLoad))
            {
                EnqueueFileEvent(e.FullPath);
            }
        }

        private async void EnqueueFileEvent(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                _logger.LogInformation($"Ignored file: {filePath}, unsupported extension.");
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));

            lock (_lock) // Ensure only one event is processed at a time
            {
                var now = DateTime.UtcNow;

                if (_trackedFiles.TryGetValue(filePath, out var lastProcessed))
                {
                    if (now - lastProcessed < _eventCooldown)
                    {
                        _logger.LogInformation($"Ignoring file event for '{filePath}' due to cooldown.");
                        return;
                    }
                }

                if (_processingFiles.ContainsKey(filePath) || _processedFiles.Contains(filePath))
                {
                    _logger.LogInformation($"File '{filePath}' is already being processed or has been processed.");
                    return;
                }

                if (IsFileReady(filePath))
                {
                    _trackedFiles[filePath] = now;
                    _processingFiles.TryAdd(filePath, true);
                    _changeQueue.Enqueue(filePath);
                    RestartDebounceTimer();
                }
                else
                {
                    _retryQueue.Enqueue(filePath);
                }
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
                if (IsFileReady(filePath))
                {
                    _changeQueue.Enqueue(filePath);
                    RestartDebounceTimer();
                }
                else
                {
                    _retryQueue.Enqueue(filePath);
                }
            }
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
