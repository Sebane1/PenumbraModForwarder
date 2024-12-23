using PenumbraModForwarder.FileMonitor.Models;

namespace PenumbraModForwarder.FileMonitor.Interfaces;

public interface IFileWatcher : IDisposable
{
    Task StartWatchingAsync(IEnumerable<string> paths);
    event EventHandler<FileMovedEvent> FileMoved;
    event EventHandler<FilesExtractedEventArgs> FilesExtracted;
}