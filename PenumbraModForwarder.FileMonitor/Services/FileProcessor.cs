using System.Text.RegularExpressions;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Models;
using Serilog;
using SevenZipExtractor;

namespace PenumbraModForwarder.FileMonitor.Services;

public sealed class FileProcessor : IFileProcessor
{
    private static readonly Regex PreDtRegex = new(@"\[?(?i)pre[\s\-]?dt\]?", RegexOptions.Compiled);

    private readonly IFileStorage _fileStorage;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public FileProcessor(
        IFileStorage fileStorage,
        IConfigurationService configurationService
    )
    {
        _fileStorage = fileStorage;
        _configurationService = configurationService;
        _logger = Log.ForContext<FileProcessor>();
    }

    public bool IsFileReady(string filePath)
    {
        if (!IsFileFullyDownloaded(filePath))
            return false;

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            // File is locked by another process
            return false;
        }
    }

    public async Task ProcessFileAsync(
        string filePath,
        CancellationToken cancellationToken,
        EventHandler<FileMovedEvent> onFileMoved,
        EventHandler<FilesExtractedEventArgs> onFilesExtracted
    )
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        var relocateFiles = (bool)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.RelocateFiles);

        if (FileExtensionsConsts.ModFileTypes.Contains(extension))
        {
            // Decide whether to physically move the file or just organize it locally
            var finalFilePath = relocateFiles
                ? MoveFile(filePath)
                : OrganizeLocalFile(filePath);

            var fileName = Path.GetFileName(finalFilePath);
            onFileMoved?.Invoke(
                this,
                new FileMovedEvent(fileName, finalFilePath, Path.GetFileNameWithoutExtension(finalFilePath))
            );
        }
        else if (FileExtensionsConsts.ArchiveFileTypes.Contains(extension))
        {
            if (await ArchiveContainsModFileAsync(filePath, cancellationToken))
            {
                // Decide whether to physically move the file or just organize it locally
                var finalFilePath = relocateFiles
                    ? MoveFile(filePath)
                    : OrganizeLocalFile(filePath);

                await ProcessArchiveFileAsync(finalFilePath, cancellationToken, onFilesExtracted);
            }
            else
            {
                _logger.Information(
                    "Archive {FilePath} doesn’t contain any recognized mod files; leaving file in place.",
                    filePath
                );
            }
        }
        else
        {
            _logger.Warning("Unhandled file type: {FullPath}", filePath);
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
                        "Detected part files for {FilePath}. Still downloading.",
                        filePath
                    );
                    return false;
                }
            }

            // Check file size stability
            const int maxChecks = 3;
            long lastSize = -1;
            for (int i = 0; i < maxChecks; i++)
            {
                var fileInfo = new FileInfo(filePath);
                var currentSize = fileInfo.Length;

                // If size is unchanged between checks, assume download complete
                if (lastSize == currentSize && currentSize != 0)
                    return true;

                lastSize = currentSize;
                Thread.Sleep(1000);
            }

            return false;
        }
        catch (IOException)
        {
            // Either file is locked or there's an issue reading
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Unexpected error checking download completeness: {FilePath}",
                filePath
            );
            return false;
        }
    }

    private async Task<bool> ArchiveContainsModFileAsync(
        string filePath,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var archiveFile = new ArchiveFile(filePath);

            var skipPreDt = (bool)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.SkipPreDt);

            var modEntries = GetModEntries(archiveFile, skipPreDt);
            return modEntries.Any();
        }
        catch (SevenZipException ex) when (ex.Message.Contains("not a known archive type"))
        {
            _logger.Warning("File {FilePath} is not recognized as a valid archive.", filePath);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Archive check canceled for {FilePath}.", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for mod files in archive: {FilePath}", filePath);
            return false;
        }
        finally
        {
            await Task.Delay(100, cancellationToken);
        }
    }

    private string MoveFile(string filePath)
    {
        var modPath = (string)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.ModFolderPath);

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var destinationFolder = Path.Combine(modPath, fileNameWithoutExt);

        // Prepare destination folder
        _fileStorage.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
        _fileStorage.CopyFile(filePath, destinationPath, overwrite: true);

        // Clean up original file
        DeleteFileWithRetry(filePath);
        _logger.Information("File moved from {SourcePath} to {DestinationPath}", filePath, destinationPath);

        return destinationPath;
    }
    
    private string OrganizeLocalFile(string filePath)
    {
        var originalDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var destinationFolder = Path.Combine(originalDirectory, fileNameWithoutExtension);

        // Create a local subdirectory named after the file (minus extension)
        _fileStorage.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
        _fileStorage.CopyFile(filePath, destinationPath, overwrite: true);

        // Clean up the original
        DeleteFileWithRetry(filePath);

        _logger.Information("File placed in subfolder: {DestinationPath}", destinationPath);
        return destinationPath;
    }

    private async Task ProcessArchiveFileAsync(
        string archivePath,
        CancellationToken cancellationToken,
        EventHandler<FilesExtractedEventArgs> onFilesExtracted
    )
    {
        var extractedFiles = new List<string>();

        try
        {
            using (var archiveFile = new ArchiveFile(archivePath))
            {
                var skipPreDt = (bool)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.SkipPreDt);
                var archiveDirectory = Path.GetDirectoryName(archivePath) ?? string.Empty;
                var modEntries = GetModEntries(archiveFile, skipPreDt);

                foreach (var entry in modEntries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var destinationPath = Path.Combine(archiveDirectory, entry.FileName);
                    var destinationDirectory = Path.GetDirectoryName(destinationPath);
                    if (destinationDirectory != null)
                        _fileStorage.CreateDirectory(destinationDirectory);

                    _logger.Information("Extracting: {FileName} to {DestPath}", entry.FileName, destinationPath);
                    entry.Extract(destinationPath);
                    extractedFiles.Add(destinationPath);
                }
            }

            await Task.Delay(100, cancellationToken);

            if (extractedFiles.Any())
            {
                onFilesExtracted?.Invoke(
                    this,
                    new FilesExtractedEventArgs(Path.GetFileName(archivePath), extractedFiles)
                );

                var shouldDelete = (bool)_configurationService.ReturnConfigValue(c => c.BackgroundWorker.AutoDelete);
                if (shouldDelete)
                {
                    DeleteFileWithRetry(archivePath);
                    _logger.Information("Archive deleted after extraction: {ArchiveFileName}", Path.GetFileName(archivePath));
                }
            }
            else
            {
                _logger.Information("No mod files found in the archive: {ArchiveFileName}", Path.GetFileName(archivePath));
            }
        }
        catch (SevenZipException ex) when (ex.Message.Contains("not a known archive type"))
        {
            _logger.Warning("Unrecognized archive format: {ArchiveFilePath}", archivePath);
            DeleteFileWithRetry(archivePath);
            _logger.Information("Deleted invalid archive: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Canceled processing of archive: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing archive: {ArchiveFileName}", Path.GetFileName(archivePath));
        }
    }

    private void DeleteFileWithRetry(string filePath, int maxAttempts = 3, int delayMs = 500)
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
                    "Attempt {Attempt} to delete file failed: {FilePath}. Retrying...",
                    attempt,
                    filePath
                );
                Thread.Sleep(delayMs);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete file: {FilePath}", filePath);
                throw;
            }
        }

        // Final attempt
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

    private List<Entry?> GetModEntries(ArchiveFile archiveFile, bool skipPreDt)
    {
        return archiveFile.Entries.Where(entry =>
        {
            var entryExtension = Path.GetExtension(entry.FileName)?.ToLowerInvariant();
            if (!FileExtensionsConsts.ModFileTypes.Contains(entryExtension))
                return false;

            if (skipPreDt && entry.FileName
                .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(dir => PreDtRegex.IsMatch(dir)))
            {
                _logger.Information("Skipping file in pre-Dt folder: {FileName}", entry.FileName);
                return false;
            }

            return true;
        }).ToList();
    }
}