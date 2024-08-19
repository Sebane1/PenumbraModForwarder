using PenumbraModForwarder.Common.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly string _extractionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Extraction";

        public ArchiveHelperService(ILogger<ArchiveHelperService> logger, IFileSelector fileSelector, IPenumbraInstallerService penumbraInstallerService, IConfigurationService configurationService, IErrorWindowService errorWindowService, IArkService arkService)
        {
            _logger = logger;
            _fileSelector = fileSelector;
            _penumbraInstallerService = penumbraInstallerService;
            _configurationService = configurationService;
            _errorWindowService = errorWindowService;
            _arkService = arkService;

            if (!Directory.Exists(_extractionPath))
            {
                Directory.CreateDirectory(_extractionPath);
            }
        }

        public void ExtractArchive(string filePath)
        {
            var files = GetFilesInArchive(filePath);

            // Check if the archive contains any .rpvsp files
            if (ContainsRolePlayVoiceFile(files))
            {
                _logger.LogInformation("File is a RolePlayVoice File");
                _arkService.InstallArkFile(filePath);
                return;
            }

            if (files.Length > 1)
            {
                if (!_configurationService.GetConfigValue(o => o.ExtractAll))
                {
                    _logger.LogInformation("Multiple files found in archive. Showing file selection dialog.");
                    var selectedFiles = _fileSelector.SelectFiles(files);
                    if (selectedFiles.Length == 0)
                    {
                        _logger.LogWarning("No files selected. Aborting extraction.");
                        return;
                    }

                    foreach (var file in selectedFiles)
                    {
                        ExtractAndInstallFile(filePath, file);
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        ExtractAndInstallFile(filePath, file);
                    }
                }
            }
            else
            {
                ExtractAndInstallFile(filePath, files[0]);
            }

            // Delete the archive after extraction
            if (_configurationService.GetConfigValue(option => option.AutoDelete))
            {
                _logger.LogInformation("Deleting archive: {0}", filePath);
                File.Delete(filePath);
            }
        }
        
        
        private bool ContainsRolePlayVoiceFile(string[] files)
        {
            return files.Any(file => file.EndsWith(".rpvsp", StringComparison.OrdinalIgnoreCase));
        }

        private void ExtractAndInstallFile(string archivePath, string filePath)
        {
            _logger.LogInformation("Extracting file: {0}", filePath);
            var extractedFile = ExtractFileFromArchive(archivePath, filePath);
            _penumbraInstallerService.InstallMod(extractedFile);
        }
        
        private string ExtractFileFromArchive(string archivePath, string filePath)
        {
            using var archive = OpenArchive(archivePath);
            // We should never get here if the archive is null
            if (archive == null) throw new InvalidOperationException("Archive could not be opened.");

            var entry = archive.Entries.FirstOrDefault(e => e.Key == filePath);
            // We should never get here if the file is not found
            if (entry == null) throw new InvalidOperationException("File not found in archive.");
            
            _logger.LogInformation("Extracting file: {0}", entry.Key);

            entry.WriteToDirectory(_extractionPath, new ExtractionOptions()
            {
                ExtractFullPath = true,
                Overwrite = true
            });
            
            _logger.LogInformation($"File: {entry.Key} extracted to: {_extractionPath}");
            
            return Path.Combine(_extractionPath, entry.Key);
        }

        private string[] GetFilesInArchive(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("File path is null or empty.");
                throw new ArgumentNullException(nameof(filePath));
            }

            var allowedExtensions = new[] {".pmp", ".ttmp2", ".ttmp", ".rpvsp"};
            var fileEntries = new HashSet<string>();
            
            _logger.LogInformation("Opening archive: {0}", filePath);

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

            _logger.LogInformation($"Attempting to open archive: {filePath}");

            try
            {
                // Determine the archive type based on the file extension
                var extension = Path.GetExtension(filePath).ToLower();
                _logger.LogInformation($"Archive extension: {extension}");
                switch (extension)
                {
                    case ".7z":
                        return SevenZipArchive.Open(filePath);
                    case ".zip":
                        return ZipArchive.Open(filePath);
                    case ".rar":
                        return RarArchive.Open(filePath);
                    default:
                        _logger.LogError($"Unsupported archive format: {extension}");
                        _errorWindowService.ShowError($"The file format {extension} is not supported.");
                        throw new NotSupportedException($"The file format {extension} is not supported.");
                }
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
