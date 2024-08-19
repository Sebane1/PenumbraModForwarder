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
        private readonly string _extractionPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Extraction";

        public ArchiveHelperService(ILogger<ArchiveHelperService> logger, IFileSelector fileSelector, IPenumbraInstallerService penumbraInstallerService)
        {
            _logger = logger;
            _fileSelector = fileSelector;
            _penumbraInstallerService = penumbraInstallerService;

            if (!Directory.Exists(_extractionPath))
            {
                Directory.CreateDirectory(_extractionPath);
            }
        }

        public void ExtractArchive(string filePath)
        {
            var files = GetFilesInArchive(filePath);
            if (files.Length > 1)
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
                    _logger.LogInformation("Extracting file: {0}", file);
                    var extractedFile = ExtractFileFromArchive(filePath, file);
                    _penumbraInstallerService.InstallMod(extractedFile);
                }
            }
            else
            {
                var extractedFile = ExtractFileFromArchive(filePath, files[0]);
                _penumbraInstallerService.InstallMod(extractedFile);
            }
            
            // Delete the archive after extraction
            _logger.LogInformation("Deleting archive: {0}", filePath);
            File.Delete(filePath);
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
                if (archive == null) throw new InvalidOperationException("Archive could not be opened.");

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
                        throw new NotSupportedException($"The file format {extension} is not supported.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open archive: {filePath}");
                throw;
            }
        }

    }
}
