using PenumbraModForwarder.BackgroundWorker.Enums;
using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.BackgroundWorker.Services;

public class ModHandlerService : IModHandlerService
{
    public ModHandlerService()
    {
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

        throw new NotSupportedException($"Unsupported file extension: {fileExtension}");
    }

    private async Task HandleModFileAsync(string filePath)
    {
        Log.Information($"Handling file: {filePath}");
    }
}