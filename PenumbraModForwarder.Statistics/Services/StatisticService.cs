using LiteDB;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Statistics.Enums;
using PenumbraModForwarder.Statistics.Interfaces;
using PenumbraModForwarder.Statistics.Models;
using Serilog;

namespace PenumbraModForwarder.Statistics.Services;

public class StatisticService : IStatisticService
{
    private readonly string _databasePath;
    private readonly IFileStorage _fileStorage;

    public StatisticService(IFileStorage fileStorage, string databasePath = "UserStats.db")
    {
        _databasePath = databasePath;
        _fileStorage = fileStorage;
    }

    public async Task IncrementStatAsync(Stat stat)
    {
        try
        {
            EnsureDatabaseExists();
            using (var db = new LiteDatabase(_databasePath))
            {
                var stats = db.GetCollection<StatRecord>("stats");
                var statRecord = stats.FindOne(x => x.Name == stat.ToString());

                if (statRecord == null)
                {
                    statRecord = new StatRecord { Name = stat.ToString(), Count = 1 };
                    stats.Insert(statRecord);
                    Log.Information($"Inserted new record for statistic '{stat}'.");
                }
                else
                {
                    statRecord.Count += 1;
                    stats.Update(statRecord);
                    Log.Information($"Updated record for statistic '{stat}' to count {statRecord.Count}.");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to increment statistic '{stat}'.");
        }
    }

    public async Task<int> GetStatCountAsync(Stat stat)
    {
        try
        {
            EnsureDatabaseExists();
            using (var db = new LiteDatabase(_databasePath))
            {
                var stats = db.GetCollection<StatRecord>("stats");
                var statRecord = stats.FindOne(x => x.Name == stat.ToString());

                if (statRecord == null)
                {
                    Log.Warning($"No records found for statistic '{stat}'.");
                    return 0;
                }

                Log.Information($"Retrieved count for statistic '{stat}': {statRecord.Count}");
                return statRecord.Count;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to retrieve statistic '{stat}'.");
            return 0;
        }
    }

    private void EnsureDatabaseExists()
    {
        if (!_fileStorage.Exists(_databasePath))
        {
            using (var db = new LiteDatabase(_databasePath))
            {
                // Initialize the database if needed
            }
            Log.Information("Database file created.");
        }
    }
}