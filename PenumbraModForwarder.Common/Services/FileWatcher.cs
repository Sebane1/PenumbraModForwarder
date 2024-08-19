using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
        private Timer _debounceTimer;
        private ConcurrentQueue<string> _changeQueue;
        private HashSet<string> _processingFiles;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);
        private readonly string[] _allowedExtensions = { ".zip", ".rar", ".7z", ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };

        public FileWatcher(ILogger<FileWatcher> logger, IConfigurationService configurationService, IFileHandlerService fileHandlerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationService = configurationService;
            _fileHandlerService = fileHandlerService;

            _changeQueue = new ConcurrentQueue<string>();
            _processingFiles = new HashSet<string>();

            _debounceTimer = new Timer(_debounceInterval.TotalMilliseconds)
            {
                AutoReset = false
            };
            _debounceTimer.Elapsed += OnDebounceTimerElapsed;

            if (_configurationService.GetConfigValue(config => config.AutoLoad))
            {
                InitializeWatcher();
            }

            _configurationService.ConfigChanged += OnConfigChange;
        }

        private void OnConfigChange(object sender, EventArgs e)
        {
            _logger.LogInformation("Config changed");
            InitializeWatcher();
        }

        private void InitializeWatcher()
        {
            _logger.LogInformation("Initializing watcher");

            // Clean up old watcher
            if (_watcher != null)
            {
                _watcher.Created -= OnFileCreated;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Dispose();
            }

            var directory = _configurationService.GetConfigValue(config => config.DownloadPath);

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                _logger.LogWarning("Directory does not exist or is invalid.");
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

            _logger.LogInformation($"Watching directory: {directory}");
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_configurationService.GetConfigValue(o => o.AutoLoad))
            {
                ProcessFileEvent(e.FullPath);
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (_configurationService.GetConfigValue(o => o.AutoLoad))
            {
                ProcessFileEvent(e.FullPath);
            }
        }

        private void ProcessFileEvent(string filePath)
        {
            if (_processingFiles.Contains(filePath)) return;

            var fileExtension = Path.GetExtension(filePath).ToLower();

            if (_allowedExtensions.Contains(fileExtension))
            {
                _changeQueue.Enqueue(filePath);
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
            else
            {
                _logger.LogInformation($"Ignored file with unsupported extension: {filePath}");
            }
        }

        private void OnDebounceTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _debounceTimer.Stop();
            ProcessFileChanges();
        }

        private void ProcessFileChanges()
        {
            while (_changeQueue.TryDequeue(out var file))
            {
                _processingFiles.Add(file);
                _logger.LogInformation($"Processing file: {file}");

                try
                {
                    _fileHandlerService.HandleFile(file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to extract archive: {file}");
                }

                _processingFiles.Remove(file);
            }
        }
    }
}
