using LiteDB;
using NLog;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.Statistics.Models;

namespace PenumbraModForwarder.Statistics.Services;

public class StatisticService : IStatisticService
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly string _databasePath;
    private readonly IFileStorage _fileStorage;
    private static readonly object _dbLock = new object();

    public StatisticService(IFileStorage fileStorage, string? databasePath = null)
    {
        _fileStorage = fileStorage;
        _databasePath = databasePath
                        ?? $@"{Common.Consts.ConfigurationConsts.ConfigurationPath}\userstats.db";

        EnsureDatabaseExists();
    }

    private TResult ExecuteDatabaseAction<TResult>(
        Func<LiteDatabase, TResult> action,
        string errorContext,
        TResult defaultValue = default)
    {
        try
        {
            lock (_dbLock)
            {
                using var database = new LiteDatabase(_databasePath);
                return action(database);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, errorContext);
            return defaultValue!;
        }
    }

    private void ExecuteDatabaseAction(
        Action<LiteDatabase> action,
        string errorContext)
    {
        try
        {
            lock (_dbLock)
            {
                using var database = new LiteDatabase(_databasePath);
                action(database);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, errorContext);
        }
    }

    public Task IncrementStatAsync(Stat stat)
    {
        ExecuteDatabaseAction(db =>
            {
                var stats = db.GetCollection<StatRecord>("stats");
                var statRecord = stats.FindOne(x => x.Name == stat.ToString());

                if (statRecord == null)
                {
                    statRecord = new StatRecord
                    {
                        Name = stat.ToString(),
                        Count = 1
                    };
                    stats.Insert(statRecord);
                    _logger.Info("Inserted new record for statistic '{Stat}'.", stat);
                }
                else
                {
                    statRecord.Count += 1;
                    stats.Update(statRecord);
                    _logger.Info("Updated record for statistic '{Stat}' to count {Count}.", stat, statRecord.Count);
                }
            },
            $"Failed to increment statistic '{stat}'.");

        return Task.CompletedTask;
    }

    public Task<int> GetStatCountAsync(Stat stat)
    {
        var result = ExecuteDatabaseAction(db =>
            {
                var stats = db.GetCollection<StatRecord>("stats");
                var statRecord = stats.FindOne(x => x.Name == stat.ToString());
                if (statRecord == null)
                {
                    _logger.Warn("No records found for statistic '{Stat}'.", stat);
                    return 0;
                }

                _logger.Info("Retrieved count for statistic '{Stat}': {Count}", stat, statRecord.Count);
                return statRecord.Count;
            },
            $"Failed to retrieve statistic '{stat}'.",
            0);

        return Task.FromResult(result);
    }

    public Task<int> GetModsInstalledTodayAsync()
    {
        var countToday = ExecuteDatabaseAction(db =>
            {
                var modInstallations = db.GetCollection<ModInstallationRecord>("mod_installations");
                var startOfToday = DateTime.UtcNow.Date;
                var count = modInstallations.Count(x => x.InstallationTime >= startOfToday);

                _logger.Info("Retrieved {Count} mods installed today.", count);
                return count;
            },
            "Failed to retrieve mods installed today.",
            0);

        return Task.FromResult(countToday);
    }

    public async Task RecordModInstallationAsync(string modName)
    {
        ExecuteDatabaseAction(db =>
            {
                var modInstallations = db.GetCollection<ModInstallationRecord>("mod_installations");
                var installationRecord = new ModInstallationRecord
                {
                    ModName = modName,
                    InstallationTime = DateTime.UtcNow
                };
                modInstallations.Insert(installationRecord);
            },
            $"Failed to record installation of mod '{modName}'.");

        await IncrementStatAsync(Stat.ModsInstalled);
    }

    public Task<ModInstallationRecord?> GetMostRecentModInstallationAsync()
    {
        var mostRecent = ExecuteDatabaseAction(db =>
            {
                var modInstallations = db.GetCollection<ModInstallationRecord>("mod_installations");
                var record = modInstallations.FindAll()
                    .OrderByDescending(m => m.InstallationTime)
                    .FirstOrDefault();

                if (record != null)
                {
                    _logger.Info(
                        "Retrieved most recent mod installation: '{ModName}' at {InstallationTime}.",
                        record.ModName,
                        record.InstallationTime
                    );
                }
                else
                {
                    _logger.Warn("No mod installations found.");
                }

                return record;
            },
            "Failed to retrieve the most recent mod installation.");

        return Task.FromResult(mostRecent);
    }

    public Task<int> GetUniqueModsInstalledCountAsync()
    {
        var uniqueCount = ExecuteDatabaseAction(db =>
            {
                var modInstallations = db.GetCollection<ModInstallationRecord>("mod_installations");
                var uniqueModNames = modInstallations
                    .FindAll()
                    .Select(x => x.ModName)
                    .Distinct();

                var modCount = uniqueModNames.Count();

                _logger.Info("Retrieved count of unique mods installed: {Count}", modCount);
                return modCount;
            },
            "Failed to retrieve count of unique mods installed.",
            0);

        return Task.FromResult(uniqueCount);
    }

    public Task<List<ModInstallationRecord>> GetAllInstalledModsAsync()
    {
        var allMods = ExecuteDatabaseAction(db =>
            {
                var modInstallations = db.GetCollection<ModInstallationRecord>("mod_installations");
                var mods = modInstallations
                    .FindAll()
                    .OrderByDescending(x => x.InstallationTime)
                    .ToList();

                _logger.Info("Retrieved {Count} mod installations from the database.", mods.Count);
                return mods;
            },
            "Failed to retrieve all installed mods.",
            new List<ModInstallationRecord>());

        return Task.FromResult(allMods);
    }

    private void EnsureDatabaseExists()
    {
        if (!_fileStorage.Exists(_databasePath))
        {
            _logger.Info("Database will be created at '{DatabasePath}'.", _databasePath);
        }
    }
}