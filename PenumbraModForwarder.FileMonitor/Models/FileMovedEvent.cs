namespace PenumbraModForwarder.FileMonitor.Models;

public class FileMovedEvent : EventArgs
{
    public string SourcePath { get; }
    public string DestinationPath { get; }

    public FileMovedEvent(string sourcePath, string destinationPath)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }
}