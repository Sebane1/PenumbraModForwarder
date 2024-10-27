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
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File does not exist: {filePath}");
                    _errorWindowService.ShowError($"File not found: {filePath}");
                    return;
                }

                var operation = new ExtractionOperation(filePath);
                _operationQueue.Enqueue(operation);
                await ProcessQueueAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error queueing extraction for {filePath}");
                _errorWindowService.ShowError($"Error processing file: {ex.Message}");
            }
        }

        private async Task ProcessQueueAsync()
        {
            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(1)))
            {
                _logger.LogInformation("Another extraction is in progress. Skipping.");
                return;
            }

            try
            {
                while (_operationQueue.TryDequeue(out var operation))
                {
                    try
                    {
                        await ProcessExtractionAsync(operation);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing extraction for {operation.FilePath}");
                        _errorWindowService.ShowError($"Error extracting file: {ex.Message}");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ProcessExtractionAsync(ExtractionOperation operation)
        {
            string[] files;
            try
            {
                files = GetFilesInArchive(operation.FilePath);
                if (files.Length == 0)
                {
                    _logger.LogWarning($"No valid files found in archive: {operation.FilePath}");
                    _errorWindowService.ShowError("No valid mod files found in archive.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading archive: {operation.FilePath}");
                _errorWindowService.ShowError($"Error reading archive: {ex.Message}");
                return;
            }

            if (HandleRolePlayVoiceFile(operation.FilePath, files))
            {
                return;
            }

            var selectedFiles = await Task.Run(() => GetSelectedFiles(files, operation.FilePath));
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                _logger.LogWarning("No files selected. Aborting extraction.");
                return;
            }

            try
            {
                _progressWindowService.ShowProgressWindow();
                await Task.Run(() => ExtractFiles(operation.FilePath, selectedFiles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during extraction: {operation.FilePath}");
                _errorWindowService.ShowError($"Extraction failed: {ex.Message}");
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
            var currentFileName = Path.GetFileName(filePath);
            _logger.LogDebug($"Starting extraction of {selectedFiles.Length} files from {currentFileName}");

            await using var archiveStream = File.OpenRead(filePath);
            using var archiveFile = new ArchiveFile(archiveStream);
            var extractedFiles = new List<string>();

            archiveFile.ExtractProgress += (sender, progress) =>
            {
                _progressWindowService.UpdateProgress(
                    currentFileName,
                    $"Extracting {progress.PercentProgress:0.00}%",
                    (int)progress.PercentProgress);
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        
                archiveFile.Extract(entry =>
                {
                    if (!selectedFiles.Contains(entry?.FileName)) return null;

                    var sanitizedFilePath = SanitizePath(entry.FileName);
                    var destinationPath = Path.Combine(_extractionPath, sanitizedFilePath);
                    var directoryPath = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    extractedFiles.Add(destinationPath);
                    return destinationPath;
                }, cts.Token);

                foreach (var extractedFile in extractedFiles)
                {
                    await Task.Run(() => InstallMod(extractedFile));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Extraction timed out");
                _errorWindowService.ShowError("Extraction timed out after 5 minutes");
            }
            finally
            {
                archiveStream.Close();
                DeleteArchiveIfNeeded(filePath);
            }
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

            using (var archiveStream = File.OpenRead(filePath))
            {
                var archiveFile = new ArchiveFile(archiveStream);
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
