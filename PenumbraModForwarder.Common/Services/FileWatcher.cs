using System.Collections.Concurrent;
using System.Timers;
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
        private FileSystemWatcher _watcher;
        private readonly IErrorWindowService _errorWindowService;
        private Timer _debounceTimer;
        private Timer _retryTimer;
        private ConcurrentQueue<string> _changeQueue;
        private ConcurrentQueue<string> _retryQueue;
        private ConcurrentDictionary<string, bool> _processingFiles;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(5);
        private readonly string[] _allowedExtensions = { ".zip", ".rar", ".7z", ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };

        public FileWatcher(ILogger<FileWatcher> logger, IConfigurationService configurationService, IFileHandlerService fileHandlerService, IErrorWindowService errorWindowService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationService = configurationService;
            _fileHandlerService = fileHandlerService;
            _errorWindowService = errorWindowService;

            _changeQueue = new ConcurrentQueue<string>();
            _retryQueue = new ConcurrentQueue<string>();
            _processingFiles = new ConcurrentDictionary<string, bool>();

            _debounceTimer = new Timer(_debounceInterval.TotalMilliseconds)
            {
                AutoReset = false
            };
            _debounceTimer.Elapsed += OnDebounceTimerElapsed;

            _retryTimer = new Timer(_retryInterval.TotalMilliseconds)
            {
                AutoReset = true
            };
            _retryTimer.Elapsed += OnRetryTimerElapsed;
            _retryTimer.Start();

            if (_configurationService.GetConfigValue(config => config.AutoLoad))
            {
                InitializeWatcher();
            }

            _configurationService.ConfigChanged += OnConfigChange;
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
                _logger.LogInformation("Previous watcher disposed.");
            }

            var directory = _configurationService.GetConfigValue(config => config.DownloadPath);
            _logger.LogInformation($"Download path from configuration: {directory}");

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
                ProcessFileEvent(e.FullPath);
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"File created: {e.FullPath}");
            if (_configurationService.GetConfigValue(o => o.AutoLoad))
            {
                ProcessFileEvent(e.FullPath);
            }
        }

        private void ProcessFileEvent(string filePath)
        {
            if (_processingFiles.ContainsKey(filePath))
            {
                _logger.LogInformation($"File '{filePath}' is already being processed.");
                return;
            }

            var fileExtension = Path.GetExtension(filePath).ToLower();
            _logger.LogInformation($"File extension: {fileExtension}");

            if (_allowedExtensions.Contains(fileExtension))
            {
                if (IsFileReady(filePath))
                {
                    _logger.LogInformation($"Enqueuing file for processing: {filePath}");
                    _changeQueue.Enqueue(filePath);
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
                else
                {
                    _logger.LogInformation($"File '{filePath}' is not ready, adding to retry queue.");
                    _retryQueue.Enqueue(filePath);
                }
            }
            else
            {
                _logger.LogInformation($"Ignored file: {filePath}, unsupported extension.");
            }
        }

        private void OnDebounceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogDebug("Debounce timer elapsed, processing queued files.");
            _debounceTimer.Stop();
            ProcessFileChanges();
        }

        private void ProcessFileChanges()
        {
            while (_changeQueue.TryDequeue(out var file))
            {
                _logger.LogDebug($"Dequeuing file: {file} for processing.");
                _processingFiles.TryAdd(file, true);

                try
                {
                    _fileHandlerService.HandleFile(file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to process file: {file}");
                    _errorWindowService.ShowError($"Failed to process file: {file}");
                }

                _processingFiles.TryRemove(file, out _);
                _logger.LogInformation($"File processing completed: {file}");
            }
        }

        private void OnRetryTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogDebug("Retry timer elapsed, checking files in retry queue.");
            ProcessRetryQueue();
        }

        private void ProcessRetryQueue()
        {
            var retryQueueCopy = new ConcurrentQueue<string>(_retryQueue);
            _retryQueue = new ConcurrentQueue<string>();

            while (retryQueueCopy.TryDequeue(out var filePath))
            {
                if (IsFileReady(filePath))
                {
                    _logger.LogDebug($"File '{filePath}' is now ready, enqueuing for processing.");
                    _changeQueue.Enqueue(filePath);
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
                else
                {
                    _logger.LogDebug($"File '{filePath}' is still not ready, requeuing.");
                    _retryQueue.Enqueue(filePath);
                }
            }
        }

        private bool IsFileReady(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                _logger.LogDebug($"File '{filePath}' is ready for processing.");
                return true;
            }
            catch (IOException)
            {
                _logger.LogDebug($"File '{filePath}' is not ready for processing.");
                return false;
            }
        }
    }
}
