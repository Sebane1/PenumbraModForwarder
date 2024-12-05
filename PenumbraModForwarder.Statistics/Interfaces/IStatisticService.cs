using PenumbraModForwarder.Statistics.Enums;

namespace PenumbraModForwarder.Statistics.Interfaces;

public interface IStatisticService
{ 
    Task IncrementStatAsync(Stat stat); 
    Task<int> GetStatCountAsync(Stat stat);
}