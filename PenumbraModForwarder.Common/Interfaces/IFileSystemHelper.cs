namespace PenumbraModForwarder.Common.Interfaces;

public interface IFileSystemHelper
{
    bool FileExists(string path);
    IEnumerable<string> GetStandardTexToolsPaths();
}