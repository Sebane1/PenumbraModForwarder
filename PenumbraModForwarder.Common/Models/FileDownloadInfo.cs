namespace PenumbraModForwarder.Common.Models;

public class FileDownloadInfo
{
    public long LastSize { get; set; }
    public DateTime LastProgressTime { get; set; }
    public DateTime StartTime { get; set; }
    public int StabilityCount { get; set; }
}