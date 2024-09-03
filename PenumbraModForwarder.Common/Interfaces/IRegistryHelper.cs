namespace PenumbraModForwarder.Common.Interfaces;

public interface IRegistryHelper
{
    public void CreateFileAssociation(IEnumerable<string> extensions, string applicationPath);
    public void RemoveFileAssociation(IEnumerable<string> extensions);
    public void AddApplicationToStartup(string appName, string appPath);
    public void RemoveApplicationFromStartup(string appName);
    public string GetTexToolRegistryValue(string keyValue);
}