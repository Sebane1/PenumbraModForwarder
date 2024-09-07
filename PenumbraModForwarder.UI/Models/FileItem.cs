namespace PenumbraModForwarder.UI.Models;

public class FileItem
{
    public string FullPath { get; set; }
    public string FileName { get; set; }

    public FileItem() { }

    public FileItem(string fullPath, string fileName)
    {
        FullPath = fullPath;
        FileName = fileName;
    }

    public override string ToString() => FileName;
}

