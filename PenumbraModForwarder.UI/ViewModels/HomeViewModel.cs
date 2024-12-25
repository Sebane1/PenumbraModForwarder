using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.UI.ViewModels;

public class HomeViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger _logger;
    private readonly IStatisticService _statisticService;
    private readonly CompositeDisposable _disposables = new();

    private ObservableCollection<InfoItem> _infoItems;

    public ObservableCollection<InfoItem> InfoItems
    {
        get => _infoItems;
        set => this.RaiseAndSetIfChanged(ref _infoItems, value);
    }

    public HomeViewModel(IStatisticService statisticService)
    {
        _logger = Log.ForContext<HomeViewModel>();
        _statisticService = statisticService;

        InfoItems = new ObservableCollection<InfoItem>();

        // TODO: Make this update in real time (Maybe Websocket firing an event - similar to how we do the file picker)
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(20))
            .SelectMany(_ => Observable.FromAsync(LoadStatisticsAsync))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Gather new data first
            var newItems = new ObservableCollection<InfoItem>();

            var modsInstalledCount = await _statisticService.GetStatCountAsync(Stat.ModsInstalled);
            var uniqueModsInstalledCount = await _statisticService.GetUniqueModsInstalledCountAsync();
            var lastModInstallation = await _statisticService.GetMostRecentModInstallationAsync();

            newItems.Add(new InfoItem("Total Mods Installed", modsInstalledCount.ToString()));
            newItems.Add(new InfoItem("Unique Mods Installed", uniqueModsInstalledCount.ToString()));

            if (lastModInstallation != null)
            {
                newItems.Add(new InfoItem("Last Mod Installed", lastModInstallation.ModName));
            }
            else
            {
                newItems.Add(new InfoItem("Last Mod Installed", "None"));
            }

            // If nothing has changed, skip an update to avoid flicker
            if (IsTheSame(InfoItems, newItems))
            {
                return;
            }

            // Otherwise, update in one pass
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
        if (existing.Count != incoming.Count)
            return false;

        // Compare each pair for a difference in Name or Value
        return !existing.Where((t, i) => !t.Name.Equals(incoming[i].Name, StringComparison.Ordinal) || !t.Value.Equals(incoming[i].Value, StringComparison.Ordinal)).Any();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}