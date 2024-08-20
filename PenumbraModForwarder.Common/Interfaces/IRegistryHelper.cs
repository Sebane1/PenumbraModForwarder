namespace PenumbraModForwarder.Common.Interfaces;

public interface IRegistryHelper
{
    public string GetTexToolsConsolePath();
    public void CreateFileAssociation(string extension, string applicationPath);
    public void RemoveFileAssociation(string extension);
}