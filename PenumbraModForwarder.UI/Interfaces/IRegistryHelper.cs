using System.Collections.Generic;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IRegistryHelper
{
    void CreateFileAssociation(IEnumerable<string> extensions, string applicationPath);
    void RemoveFileAssociation(IEnumerable<string> extensions);
    void AddApplicationToStartup(string appName, string appPath);
    void RemoveApplicationFromStartup(string appName);
}