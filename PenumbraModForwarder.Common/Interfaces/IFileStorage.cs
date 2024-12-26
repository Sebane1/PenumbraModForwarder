namespace PenumbraModForwarder.Common.Interfaces;

public interface IFileStorage
{
    bool Exists(string path);
    string Read(string path);
    void Write(string path, string content);
    void CreateDirectory(string path);
    void Delete(string path);
    void DeleteDirectory(string path);
    Stream CreateFile(string path);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);
    void WriteAllText(string path, string content);
}