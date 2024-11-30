namespace PenumbraModForwarder.Common.Interfaces;

public interface IFileStorage
{
    bool Exists(string path);
    string Read(string path);
    void Write(string path, string content);
    void CreateDirectory(string path);
}