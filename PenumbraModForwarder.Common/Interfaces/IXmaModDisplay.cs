using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IXmaModDisplay
{
    Task<List<XmaMods>> GetRecentMods();
}