using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IXmaModDisplay
{
    Task<List<XmaMods>> GetRecentMods();
    Task<string?> GetModDownloadLinkAsync(string modUrl);
}