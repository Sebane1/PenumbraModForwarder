using PenumbraModForwarder.Statistics.Enums;
using PenumbraModForwarder.Statistics.Models;

namespace PenumbraModForwarder.Statistics.Interfaces;

public interface IStatisticService
{ 
    Task IncrementStatAsync(Stat stat);
    Task<int> GetStatCountAsync(Stat stat);
    Task RecordModInstallationAsync(string modName);
    Task<ModInstallationRecord?> GetMostRecentModInstallationAsync();
}