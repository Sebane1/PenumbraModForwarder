using System;
using System.Collections.ObjectModel;
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

        // Set up polling to load statistics immediately and every 20 seconds
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
            // Clear existing items
            InfoItems.Clear();

            // Retrieve statistics
            var modsInstalledCount = await _statisticService.GetStatCountAsync(Stat.ModsInstalled);
            var uniqueModsInstalledCount = await _statisticService.GetUniqueModsInstalledCountAsync();
            var lastModInstallation = await _statisticService.GetMostRecentModInstallationAsync();

            // Update InfoItems with statistics
            InfoItems.Add(new InfoItem("Total Mods Installed", modsInstalledCount.ToString()));
            InfoItems.Add(new InfoItem("Unique Mods Installed", uniqueModsInstalledCount.ToString()));

            if (lastModInstallation != null)
            {
                InfoItems.Add(new InfoItem("Last Mod Installed", lastModInstallation.ModName));
            }
            else
            {
                InfoItems.Add(new InfoItem("Last Mod Installed", "None"));
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load statistics in HomeViewModel.");
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}