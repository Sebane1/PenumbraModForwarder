using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IStatisticService
{ 
    Task IncrementStatAsync(Stat stat);
    Task<int> GetStatCountAsync(Stat stat);
    Task RecordModInstallationAsync(string modName);
    Task<ModInstallationRecord?> GetMostRecentModInstallationAsync();
    Task<int> GetUniqueModsInstalledCountAsync();
    Task<List<ModInstallationRecord>> GetAllInstalledModsAsync();
}