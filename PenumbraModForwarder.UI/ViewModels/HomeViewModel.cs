using System;
using System.Collections.ObjectModel;
using PenumbraModForwarder.Common.Enums;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;
using Serilog;

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

        // Load statistics asynchronously
        LoadStatisticsAsync();
    }

    private async void LoadStatisticsAsync()
    {
        try
        {
            // Retrieve statistics
            var modsInstalledCount = await _statisticService.GetStatCountAsync(Stat.ModsInstalled);
            var lastModInstallation = await _statisticService.GetMostRecentModInstallationAsync();

            // Update InfoItems with statistics
            InfoItems.Add(new InfoItem("Mods Installed", modsInstalledCount.ToString()));

            if (lastModInstallation != null)
            {
                InfoItems.Add(new InfoItem("Last Mod Installed", lastModInstallation.ModName));
            }

            // Add more statistics as needed
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load statistics in HomeViewModel.");
        }
    }
}