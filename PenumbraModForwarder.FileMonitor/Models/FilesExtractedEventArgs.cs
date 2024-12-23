namespace PenumbraModForwarder.FileMonitor.Models;

public class FilesExtractedEventArgs : EventArgs
{
    public string ArchiveFileName { get; }
    public List<string> ExtractedFilePaths { get; }

    public FilesExtractedEventArgs(string archiveFileName, List<string> extractedFilePaths)
    {
        ArchiveFileName = archiveFileName;
        ExtractedFilePaths = extractedFilePaths;
    }
}