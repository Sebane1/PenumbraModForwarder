using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Interfaces;
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
    private readonly IDownloadManagerService _downloadManagerService;
    private readonly CompositeDisposable _disposables = new();
    private readonly SemaphoreSlim _statsSemaphore = new(1, 1);
    
    private readonly IWebSocketClient _webSocketClient;

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

    public HomeViewModel(
        IStatisticService statisticService,
        IXmaModDisplay xmaModDisplay,
        IWebSocketClient webSocketClient, IDownloadManagerService downloadManagerService)
    {
        _logger = Log.ForContext<HomeViewModel>();
        _statisticService = statisticService;
        _xmaModDisplay = xmaModDisplay;
        _webSocketClient = webSocketClient;
        _downloadManagerService = downloadManagerService;

        InfoItems = new ObservableCollection<InfoItem>();
        RecentMods = new ObservableCollection<XmaMods>();

        // Subscribe to ModInstalled event so we can refresh stats immediately.
        _webSocketClient.ModInstalled += OnModInstalled;

        _ = LoadStatisticsAsync();

        Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(5))
            .SelectMany(_ => Observable.FromAsync(RefreshRecentModsAsync))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);
    }
    
    private async void OnModInstalled(object sender, EventArgs e)
    {
        RxApp.MainThreadScheduler.ScheduleAsync(async (_, __) =>
        {
            await LoadStatisticsAsync();
        });
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

    public async Task DownloadModsAsync(XmaMods mod)
    {
        await _downloadManagerService.DownloadModsAsync(mod);
    }

    private async Task LoadStatisticsAsync()
    {
        if (!await _statsSemaphore.WaitAsync(TimeSpan.FromSeconds(10)))
            return;

        try
        {
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
        _webSocketClient.ModInstalled -= OnModInstalled;
        _disposables.Dispose();
    }
}