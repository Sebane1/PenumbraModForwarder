using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using SevenZipExtractor;
using System.IO;
using System.Threading.Tasks;

namespace PenumbraModForwarder.Common.Services
{
    public class ArchiveHelperService : IArchiveHelperService
    {
        private readonly ILogger<ArchiveHelperService> _logger;
        private readonly IFileSelector _fileSelector;
        private readonly IPenumbraInstallerService _penumbraInstallerService;
        private readonly IConfigurationService _configurationService;
        private readonly IErrorWindowService _errorWindowService;
        private readonly IArkService _arkService;
        private readonly IProgressWindowService _progressWindowService;

        private readonly string _extractionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"PenumbraModForwarder\Extraction");
        private readonly ConcurrentQueue<ExtractionOperation> _operationQueue = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _isFileSelectionWindowOpen = false;

        public ArchiveHelperService(
            ILogger<ArchiveHelperService> logger,
            IFileSelector fileSelector,
            IPenumbraInstallerService penumbraInstallerService,
            IConfigurationService configurationService,
            IErrorWindowService errorWindowService,
            IArkService arkService,
            IProgressWindowService progressWindowService)
        {
            _logger = logger;
            _fileSelector = fileSelector;
            _penumbraInstallerService = penumbraInstallerService;
            _configurationService = configurationService;
            _errorWindowService = errorWindowService;
            _arkService = arkService;
            _progressWindowService = progressWindowService;

            if (!Directory.Exists(_extractionPath))
            {
                Directory.CreateDirectory(_extractionPath);
            }
        }

        public async Task QueueExtractionAsync(string filePath)
        {
            var operation = new ExtractionOperation(filePath);
            _operationQueue.Enqueue(operation);
            await ProcessQueueAsync();
        }

        private async Task ProcessQueueAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                while (_operationQueue.TryDequeue(out var operation))
                {
                    await ProcessExtractionAsync(operation);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ProcessExtractionAsync(ExtractionOperation operation)
        {
            var files = GetFilesInArchive(operation.FilePath);

            if (HandleRolePlayVoiceFile(operation.FilePath, files))
            {
                return;
            }

            var selectedFiles = GetSelectedFiles(files, operation.FilePath);
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                _logger.LogWarning("No files selected. Aborting extraction.");
                return;
            }

            _progressWindowService.ShowProgressWindow();

            try
            {
                // Use asynchronous I/O with batching to process file extraction
                await Task.Run(() => ExtractFiles(operation.FilePath, selectedFiles));
            }
            finally
            {
                _progressWindowService.CloseProgressWindow();
            }
        }

        private string[] GetSelectedFiles(string[] files, string filePath)
        {
            if (files.Length <= 1 || _configurationService.GetConfigValue(o => o.ExtractAll)) return files;

            var selectedFiles = HandleFileSelection(filePath, files);
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                return null;
            }
            return selectedFiles;
        }

        private string[] HandleFileSelection(string filePath, string[] files)
        {
            _logger.LogInformation("Multiple files found in archive. Showing file selection dialog.");
            var fileName = Path.GetFileName(filePath);

            if (_isFileSelectionWindowOpen)
            {
                _logger.LogInformation("File selection window already open. Queueing operation.");
                return null;
            }

            _isFileSelectionWindowOpen = true;

            try
            {
                return _fileSelector.SelectFiles(files, fileName);
            }
            finally
            {
                _isFileSelectionWindowOpen = false;
            }
        }

        private async Task ExtractFiles(string filePath, string[] selectedFiles)
        {
            using var archiveFile = new ArchiveFile(filePath);
            var currentFileName = Path.GetFileName(filePath);
            _logger.LogDebug($"Extracting {selectedFiles.Length} files.");

            // Subscribe to the progress event to update the progress bar
            archiveFile.ExtractProgress += (sender, progress) =>
            {
                _logger.LogDebug($"Progress: {progress.PercentProgress:0.00}%");
                _progressWindowService.UpdateProgress(currentFileName, $"Extracting {progress.PercentProgress:00}%", (int)progress.PercentProgress);
            };

            var token = new CancellationToken();

            try
            {
                archiveFile.Extract(entry =>
                {
                    if (!selectedFiles.Contains(entry?.FileName)) return null;

                    var sanitizedFilePath = SanitizePath(entry?.FileName);
                    var destinationPath = Path.Combine(_extractionPath, sanitizedFilePath);
                    var directoryPath = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    return destinationPath;
                }, token);

                // Process the extracted files
                foreach (var entry in selectedFiles)
                {
                    var destinationPath = Path.Combine(_extractionPath, entry);
                    InstallMod(destinationPath);
                }
            }
            finally
            {
                // Explicitly dispose of the archive file to release any locks on the archive
                archiveFile.Dispose();
            }

            // Optionally delete the archive after extraction if configured
            DeleteArchiveIfNeeded(filePath);
        }

        
        private void InstallMod(string extractedFile)
        {
            _penumbraInstallerService.InstallMod(extractedFile);
            _logger.LogInformation($"Installed mod from: {extractedFile}");
        }

        private bool HandleRolePlayVoiceFile(string filePath, string[] files)
        {
            if (ContainsRolePlayVoiceFile(files))
            {
                _logger.LogDebug("File is a RolePlayVoice File");
                _arkService.InstallArkFile(filePath);
                return true;
            }
            return false;
        }

        private void DeleteArchiveIfNeeded(string filePath)
        {
            if (!_configurationService.GetConfigValue(option => option.AutoDelete)) return;
            _logger.LogDebug("Deleting archive: {0}", filePath);

            var maxWaitTime = 10000;  
            var waitInterval = 500;   
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            while (stopWatch.ElapsedMilliseconds < maxWaitTime)
            {
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    stream.Close();
                    File.Delete(filePath);
                    _logger.LogInformation("Successfully deleted archive: {0}", filePath);
                    return;
                }
                catch (IOException)
                {
                    _logger.LogDebug("File is still locked. Waiting...");
                    Task.Delay(waitInterval).Wait();
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError("Permission error when trying to delete archive: {0}. Error: {1}", filePath, ex.Message);
                    return;
                }
            }

            _logger.LogError("Failed to delete archive after waiting {0} seconds: {1}", maxWaitTime / 1000, filePath);
        }

        private bool ContainsRolePlayVoiceFile(string[] files)
        {
            return files.Any(file => file.EndsWith(".rpvsp", StringComparison.OrdinalIgnoreCase));
        }

        private string SanitizePath(string path)
        {
            return Path.GetInvalidPathChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public virtual string[] GetFilesInArchive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("File path is null or empty.");
                throw new ArgumentNullException(nameof(filePath));
            }

            var allowedExtensions = new[] { ".pmp", ".ttmp2", ".ttmp", ".rpvsp" };
            var fileEntries = new HashSet<string>();

            _logger.LogDebug("Opening archive: {0}", filePath);

            using (var archiveFile = new ArchiveFile(filePath))
            {
                foreach (var entry in archiveFile.Entries)
                {
                    var extension = Path.GetExtension(entry.FileName).ToLower();
                    if (allowedExtensions.Contains(extension))
                    {
                        fileEntries.Add(entry.FileName);
                    }
                }
            }

            return fileEntries.ToArray();
        }
    }
}
