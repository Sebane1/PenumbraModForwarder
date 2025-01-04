using PenumbraModForwarder.FileMonitor.Models;

namespace PenumbraModForwarder.FileMonitor.Interfaces;

public interface IFileQueueProcessor
{
    event EventHandler<FileMovedEvent> FileMoved;
    event EventHandler<FilesExtractedEventArgs> FilesExtracted;

    void EnqueueFile(string fullPath);
    void RenameFileInQueue(string oldPath, string newPath);

    Task LoadStateAsync();
    void PersistState();
    void StartProcessing();
}