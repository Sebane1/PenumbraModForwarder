namespace PenumbraModForwarder.Common.Interfaces;

public interface IRegistryHelper
{
    public void CreateFileAssociation(string extension, string applicationPath);
    public void RemoveFileAssociation(string extension);
    public void AddApplicationToStartup(string appName, string appPath);
    public void RemoveApplicationFromStartup(string appName);
}