using LiteDB;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Statistics.Services;

public class StatisticService : IStatisticService
{
    private readonly string _databasePath;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger _logger;
    private static readonly object _dbLock = new object();

    public StatisticService(IFileStorage fileStorage, string? databasePath = null)
    {
        _databasePath = databasePath ?? $@"{Common.Consts.ConfigurationConsts.ConfigurationPath}\userstats.db";
        _fileStorage = fileStorage;
        _logger = Log.ForContext<StatisticService>();

        EnsureDatabaseExists();
    }

    public Task IncrementStatAsync(Stat stat)
    {
        try
        {
            lock (_dbLock)
            {
                using (var database = new LiteDatabase(_databasePath))
                {
                    var stats = database.GetCollection<StatRecord>("stats");
                    var statRecord = stats.FindOne(x => x.Name == stat.ToString());

                    if (statRecord == null)
                    {
                        statRecord = new StatRecord { Name = stat.ToString(), Count = 1 };
                        stats.Insert(statRecord);
                        _logger.Information("Inserted new record for statistic `{Stat}`.", stat);
                    }
                    else
                    {
                        statRecord.Count += 1;
                        stats.Update(statRecord);
                        _logger.Information("Updated record for statistic `{Stat}` to count {Count}.", stat, statRecord.Count);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to increment statistic `{Stat}`.", stat);
        }

        return Task.CompletedTask;
    }

    public Task<int> GetStatCountAsync(Stat stat)
    {
        try
        {
            lock (_dbLock)
            {
                using (var database = new LiteDatabase(_databasePath))
                {
                    var stats = database.GetCollection<StatRecord>("stats");
                    var statRecord = stats.FindOne(x => x.Name == stat.ToString());

                    if (statRecord == null)
                    {
                        _logger.Warning("No records found for statistic `{Stat}`.", stat);
                        return Task.FromResult(0);
                    }

                    _logger.Information("Retrieved count for statistic `{Stat}`: {Count}", stat, statRecord.Count);
                    return Task.FromResult(statRecord.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve statistic `{Stat}`.", stat);
            return Task.FromResult(0);
        }
    }

    public Task RecordModInstallationAsync(string modName)
    {
        try
        {
            lock (_dbLock)
            {
                using (var database = new LiteDatabase(_databasePath))
                {
                    var modInstallations = database.GetCollection<ModInstallationRecord>("mod_installations");
                    var installationRecord = new ModInstallationRecord
                    {
                        ModName = modName,
                        InstallationTime = DateTime.UtcNow
                    };

                    modInstallations.Insert(installationRecord);
                }
            }
            
            return IncrementStatAsync(Stat.ModsInstalled);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to record installation of mod `{ModName}`.", modName);
            return Task.CompletedTask;
        }
    }

    public Task<ModInstallationRecord?> GetMostRecentModInstallationAsync()
    {
        try
        {
            lock (_dbLock)
            {
                using (var database = new LiteDatabase(_databasePath))
                {
                    var modInstallations = database.GetCollection<ModInstallationRecord>("mod_installations");
                    var mostRecentInstallation = modInstallations
                        .FindAll()
                        .OrderByDescending(m => m.InstallationTime)
                        .FirstOrDefault();

                    if (mostRecentInstallation != null)
                    {
                        _logger.Information("Retrieved most recent mod installation: `{ModName}` at {InstallationTime}.",
                            mostRecentInstallation.ModName, mostRecentInstallation.InstallationTime);
                        return Task.FromResult<ModInstallationRecord?>(mostRecentInstallation);
                    }
                    else
                    {
                        _logger.Warning("No mod installations found.");
                        return Task.FromResult<ModInstallationRecord?>(null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve the most recent mod installation.");
            return Task.FromResult<ModInstallationRecord?>(null);
        }
    }

    private void EnsureDatabaseExists()
    {
        if (!_fileStorage.Exists(_databasePath))
        {
            _logger.Information("Database will be created at `{DatabasePath}`.", _databasePath);
        }
    }
}