using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class FileStorage : IFileStorage
{
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public string Read(string path)
    {
        if (!Exists(path))
        {
            throw new FileNotFoundException($"The file at path {path} does not exist.");
        }
        return File.ReadAllText(path);
    }

    public void Write(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}