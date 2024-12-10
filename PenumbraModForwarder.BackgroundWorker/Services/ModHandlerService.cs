using PenumbraModForwarder.BackgroundWorker.Enums;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class ModHandlerService : IModHandlerService
{
    private readonly IArchiveExtractionService _archiveExtractionService;

    public ModHandlerService(IArchiveExtractionService archiveExtractionService)
    {
        _archiveExtractionService = archiveExtractionService;
    }

    public async Task HandleFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        }

        var fileType = GetFileType(filePath);

        switch (fileType)
        {
            case FileType.ModFile:
                await HandleModFileAsync(filePath);
                break;
            case FileType.Archive:
                await HandleArchiveFileAsync(filePath);
                break;
            default:
                throw new InvalidOperationException($"Unhandled file type: {fileType}");
        }
    }

    private FileType GetFileType(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant();

        if (FileExtensionsConsts.ModFileTypes.Contains(fileExtension))
        {
            return FileType.ModFile;
        }

        if (FileExtensionsConsts.ArchiveFileTypes.Contains(fileExtension))
        {
            return FileType.Archive;
        }

        throw new NotSupportedException($"Unsupported file extension: {fileExtension}");
    }

    private async Task HandleModFileAsync(string filePath)
    {
        throw new NotImplementedException();
    }

    private async Task HandleArchiveFileAsync(string filePath)
    {
        throw new NotImplementedException();
    }
}