namespace PenumbraModForwarder.FileMonitor.Models;

public class FileMovedEvent : EventArgs
{
    public string FileName { get; set; }
    public string SourcePath { get; }
    public string DestinationPath { get; }

    public FileMovedEvent(string sourcePath, string destinationPath, string fileName)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        FileName = fileName;
    }
}