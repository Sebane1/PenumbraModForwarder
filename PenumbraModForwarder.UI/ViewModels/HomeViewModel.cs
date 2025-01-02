using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        // Retrieve and store the recent mods when HomeView starts
        Observable.FromAsync(RefreshRecentModsAsync)
            .Subscribe()
            .DisposeWith(_disposables);

        // Periodically load statistics every 20 seconds
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(20))
            .SelectMany(_ => Observable.FromAsync(LoadStatisticsAsync))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private async Task RefreshRecentModsAsync()
    {
        try
        {
            var mods = await _xmaModDisplay.GetRecentMods();
            
            var distinctMods = mods.GroupBy(mod => mod.ModUrl).Select(g => g.First()).ToList();
            
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
    }


    private async Task LoadStatisticsAsync()
    {
        try
        {
            var newItems = new ObservableCollection<InfoItem>();

            var modsInstalledCount = await _statisticService.GetStatCountAsync(Stat.ModsInstalled);
            var uniqueModsInstalledCount = await _statisticService.GetUniqueModsInstalledCountAsync();
            var lastModInstallation = await _statisticService.GetMostRecentModInstallationAsync();

            newItems.Add(new InfoItem("Total Mods Installed", modsInstalledCount.ToString()));
            newItems.Add(new InfoItem("Unique Mods Installed", uniqueModsInstalledCount.ToString()));
            newItems.Add(lastModInstallation != null
                ? new InfoItem("Last Mod Installed", lastModInstallation.ModName)
                : new InfoItem("Last Mod Installed", "None"));

            // Remove duplicates (Name/Value duplicates)
            var distinctByNameAndValue = newItems
                .GroupBy(item => (item.Name, item.Value))
                .Select(group => group.First())
                .ToList();

            newItems.Clear();
            foreach (var i in distinctByNameAndValue)
            {
                newItems.Add(i);
            }

            // Only update InfoItems if something changed
            if (IsTheSame(InfoItems, newItems))
                return;

            InfoItems.Clear();
            foreach (var item in newItems)
            {
                InfoItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load statistics in HomeViewModel.");
        }
    }

    private bool IsTheSame(
        ObservableCollection<InfoItem> existing,
        ObservableCollection<InfoItem> incoming)
    {
        if (existing.Count != incoming.Count) return false;

        // Compare each pair for a difference in Name or Value
        return !existing
            .Where((t, i) =>
                !t.Name.Equals(incoming[i].Name, StringComparison.Ordinal)
                || !t.Value.Equals(incoming[i].Value, StringComparison.Ordinal))
            .Any();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}