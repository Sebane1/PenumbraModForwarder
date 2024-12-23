﻿using LiteDB;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Statistics.Enums;
using PenumbraModForwarder.Statistics.Interfaces;
using PenumbraModForwarder.Statistics.Models;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.Statistics.Services;

public class StatisticService : IStatisticService
{
    private readonly string _databasePath;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger _logger;

    public StatisticService(IFileStorage fileStorage, string databasePath = "UserStats.db")
    {
        _databasePath = databasePath;
        _fileStorage = fileStorage;
        _logger = Log.ForContext<StatisticService>();
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to increment statistic `{Stat}`.", stat);
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
                    _logger.Warning("No records found for statistic `{Stat}`.", stat);
                    return 0;
                }

                _logger.Information("Retrieved count for statistic `{Stat}`: {Count}", stat, statRecord.Count);
                return statRecord.Count;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to retrieve statistic `{Stat}`.", stat);
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
            _logger.Information("Database file created.");
        }
    }
}