﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.UI.ViewModels;

public class HomeViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger _logger;
    private readonly IStatisticService _statisticService;
    private readonly IXmaModDisplay _xmaModDisplay;
    private readonly CompositeDisposable _disposables = new();
    private readonly SemaphoreSlim _statsSemaphore = new(1, 1);

    private ObservableCollection<InfoItem> _infoItems;
    public ObservableCollection<InfoItem> InfoItems
    {
        get => _infoItems;
        set => this.RaiseAndSetIfChanged(ref _infoItems, value);
    }

    private ObservableCollection<XmaMods> _recentMods;
    public ObservableCollection<XmaMods> RecentMods
    {
        get => _recentMods;
        set => this.RaiseAndSetIfChanged(ref _recentMods, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public ReactiveCommand<XmaMods, Unit> OpenModLinkCommand { get; }

    public HomeViewModel(
        IStatisticService statisticService,
        IXmaModDisplay xmaModDisplay)
    {
        _logger = Log.ForContext<HomeViewModel>();
        _statisticService = statisticService;
        _xmaModDisplay = xmaModDisplay;

        InfoItems = new ObservableCollection<InfoItem>();
        RecentMods = new ObservableCollection<XmaMods>();

        _ = LoadStatisticsAsync();

        OpenModLinkCommand = ReactiveCommand.Create<XmaMods>(mod =>
        {
            if (!string.IsNullOrEmpty(mod.ModUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(mod.ModUrl)
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to open the browser for: {ModUrl}", mod.ModUrl);
                }
            }
        });

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(5))
            .SelectMany(_ => Observable.FromAsync(RefreshRecentModsAsync))
            .Merge(Observable.Timer(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20))
                .SelectMany(_ => Observable.FromAsync(LoadStatisticsAsync)))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private async Task RefreshRecentModsAsync()
    {
        try
        {
            IsLoading = true;

            var mods = await _xmaModDisplay.GetRecentMods();
            var distinctMods = mods
                .GroupBy(mod => mod.ModUrl)
                .Select(g => g.First())
                .ToList();

            RecentMods.Clear();
            foreach (var mod in distinctMods)
            {
                RecentMods.Add(mod);
            }

            _logger.Information("Successfully refreshed recent mods.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unable to retrieve or log recent mods");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStatisticsAsync()
    {
        if (!await _statsSemaphore.WaitAsync(TimeSpan.FromSeconds(10)))
            return;

        try
        {
            // Create a new collection
            var newItems = new ObservableCollection<InfoItem>
            {
                new("Total Mods Installed", (await _statisticService.GetStatCountAsync(Stat.ModsInstalled)).ToString()),
                new("Unique Mods Installed", (await _statisticService.GetUniqueModsInstalledCountAsync()).ToString())
            };
            
            var modsInstalledToday = await _statisticService.GetModsInstalledTodayAsync();
            newItems.Add(new InfoItem("Mods Installed Today", modsInstalledToday.ToString()));

            var lastModInstallation = await _statisticService.GetMostRecentModInstallationAsync();
            newItems.Add(lastModInstallation != null
                ? new InfoItem("Last Mod Installed", lastModInstallation.ModName)
                : new InfoItem("Last Mod Installed", "None"));

            // Replace the old collection
            InfoItems = newItems;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load statistics in HomeViewModel.");
        }
        finally
        {
            _statsSemaphore.Release();
        }
    }


    public void Dispose()
    {
        _disposables.Dispose();
    }
}
