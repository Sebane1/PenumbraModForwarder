using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Interfaces;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Helpers;
using PenumbraModForwarder.Common.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

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

        private readonly string _extractionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                  @"\PenumbraModForwarder\Extraction";

        // Queue and semaphore for managing operations
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

            // If there are multiple files, handle file selection first
            var selectedFiles = files;

            if (files.Length > 1 && !_configurationService.GetConfigValue(o => o.ExtractAll))
            {
                selectedFiles = HandleFileSelection(operation.FilePath, files);
                if (selectedFiles == null || selectedFiles.Length == 0)
                {
                    _logger.LogWarning("No files selected. Aborting extraction.");
                    return;
                }
            }

            // Only show the progress window after file selection
            _progressWindowService.ShowProgressWindow();

            try
            {
                // Proceed with extraction
                await Task.Run(() => ExtractFiles(operation.FilePath, selectedFiles));
            }
            finally
            {
                _progressWindowService.CloseProgressWindow();
            }
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

        private void ExtractFiles(string filePath, string[] selectedFiles)
        {
            var allFilesExtractedSuccessfully = false;
            var count = 0;
            var totalFiles = selectedFiles.Length;

            foreach (var file in selectedFiles)
            {
                _logger.LogInformation("Extracting file {0}/{1}: {2}", count + 1, totalFiles, file);
                count++;
                var report = ExtractAndInstallFile(filePath, file);
                if (report)
                {
                    allFilesExtractedSuccessfully = true;
                }
            }

            // Delete the archive only if all files were extracted successfully
            if (allFilesExtractedSuccessfully)
            {
                _logger.LogInformation("All files extracted successfully. Deleting archive.");
                DeleteArchiveIfNeeded(filePath);
            }
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
            if (_configurationService.GetConfigValue(option => option.AutoDelete))
            {
                _logger.LogDebug("Deleting archive: {0}", filePath);
                File.Delete(filePath);
            }
        }

        private bool ContainsRolePlayVoiceFile(string[] files)
        {
            return files.Any(file => file.EndsWith(".rpvsp", StringComparison.OrdinalIgnoreCase));
        }

        private bool ExtractAndInstallFile(string archivePath, string filePath)
        {
            var extractedFile = ExtractFileFromArchive(archivePath, filePath);
            if (string.IsNullOrEmpty(extractedFile))
            {
                return false;
            }
            _penumbraInstallerService.InstallMod(extractedFile);
            return true;
        }

        private string ExtractFileFromArchive(string archivePath, string filePath)
        {
            using var archive = OpenArchive(archivePath);
            if (archive == null) throw new InvalidOperationException("Archive could not be opened.");

            var entry = archive.Entries.FirstOrDefault(e => e.Key == filePath);
            if (entry == null) throw new InvalidOperationException("File not found in archive.");

            _logger.LogDebug("Extracting file: {0}", entry.Key);

            var destinationPath = Path.Combine(_extractionPath, entry.Key);

            // Ensure the directory exists before writing the file
            var directoryPath = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var totalSize = entry.Size;
            var fileName = Path.GetFileName(entry.Key);
            try
            {
                using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    // Wrap the destination stream with ProgressStream
                    using (var progressStream = new ProgressStream(destinationStream, totalSize, new Progress<double>(percentage =>
                           {
                               _progressWindowService.UpdateProgress(fileName, "Extracting", (int)percentage);
                           })))
                    {
                        entry.WriteTo(progressStream); // Write to the progress stream
                    }
                }

                _logger.LogInformation($"File: {entry.Key} extracted to: {_extractionPath}");
                _progressWindowService.CloseProgressWindow();

                return destinationPath;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "Failed to extract file: {0}", entry.Key);
                _errorWindowService.ShowError($"{fileName} is encrypted.\nPenumbra Mod Forwarder doesn't support password protected.\nPlease close this window and extract the file manually.");
                return null;
            }
        }

        public virtual string[] GetFilesInArchive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("File path is null or empty.");
                throw new ArgumentNullException(nameof(filePath));
            }

            var allowedExtensions = new[] {".pmp", ".ttmp2", ".ttmp", ".rpvsp"};
            var fileEntries = new HashSet<string>();

            _logger.LogDebug("Opening archive: {0}", filePath);

            using (var archive = OpenArchive(filePath))
            {
                if (archive == null)
                {
                    _errorWindowService.ShowError("Failed to open archive.");
                    throw new InvalidOperationException("Archive could not be opened.");
                }

                foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
                {
                    var extension = Path.GetExtension(entry.Key).ToLower();
                    if (allowedExtensions.Contains(extension))
                    {
                        fileEntries.Add(entry.Key);
                    }
                }
            }

            return fileEntries.ToArray();
        }

        private IArchive OpenArchive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("File path is null or empty.");
                _errorWindowService.ShowError("File path is null or empty.");
                throw new ArgumentNullException(nameof(filePath));
            }

            _logger.LogDebug($"Attempting to open archive: {filePath}");

            try
            {
                var extension = Path.GetExtension(filePath).ToLower();
                _logger.LogInformation($"Archive extension: {extension}");

                return extension switch
                {
                    ".7z" => SevenZipArchive.Open(filePath),
                    ".zip" => ZipArchive.Open(filePath),
                    ".rar" => RarArchive.Open(filePath),
                    _ => throw new NotSupportedException($"The file format {extension} is not supported.")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open archive: {filePath}");
                _errorWindowService.ShowError($"Failed to open archive: {filePath}");
                throw;
            }
        }
    }
}
