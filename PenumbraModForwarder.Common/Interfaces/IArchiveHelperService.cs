namespace PenumbraModForwarder.Common.Interfaces;

public interface IArchiveHelperService
{
    public Task QueueExtractionAsync(string filePath);
    public string[] GetFilesInArchive(string filePath);
}