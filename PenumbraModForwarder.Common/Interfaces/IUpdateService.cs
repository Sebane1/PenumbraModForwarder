namespace PenumbraModForwarder.Common.Interfaces;

public interface IUpdateService
{
    Task<List<string>> GetUpdateZipLinksAsync(string currentVersion);
    Task<bool> NeedsUpdateAsync(string currentVersion);
    Task<string> GetMostRecentVersionAsync();
}