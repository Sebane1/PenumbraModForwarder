using PenumbraModForwarder.FileMonitor.Models;

namespace PenumbraModForwarder.FileMonitor.Interfaces;

public interface IFileProcessor
{
    bool IsFileReady(string filePath);

    Task ProcessFileAsync(
        string filePath,
        CancellationToken cancellationToken,
        EventHandler<FileMovedEvent> onFileMoved,
        EventHandler<FilesExtractedEventArgs> onFilesExtracted
    );
}