using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PenumbraModForwarder.UI.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IStatisticService _statisticService;

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

        // Asynchronous initialization
        LoadStatisticsAsync();
    }

    private async void LoadStatisticsAsync()
    {
        try
        {
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
}