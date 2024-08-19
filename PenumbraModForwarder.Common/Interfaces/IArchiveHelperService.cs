namespace PenumbraModForwarder.Common.Interfaces;

public interface IArchiveHelperService
{
    public void ExtractArchive(string filePath);
    public string ExtractFileFromArchive(string archivePath, string filePath);
}