using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class ModsViewModel : ViewModelBase, IDisposable
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IStatisticService _statisticService;
    private readonly CompositeDisposable _disposables = new();

    private ObservableCollection<ModInstallationRecord> _installedMods;
    public ObservableCollection<ModInstallationRecord> InstalledMods
    {
        get => _installedMods;
        set => this.RaiseAndSetIfChanged(ref _installedMods, value);
    }

    public ModsViewModel(IStatisticService statisticService)
    {
        _statisticService = statisticService;
        InstalledMods = new ObservableCollection<ModInstallationRecord>();

        // Periodically refresh installed mods
        Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
            .SelectMany(_ => Observable.FromAsync(LoadInstalledModsAsync))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);
    }

    private async Task LoadInstalledModsAsync()
    {
        try
        {
            var fetchedMods = await _statisticService.GetAllInstalledModsAsync();

            // Check if the newly fetched mods are the same as the current list
            if (AreSame(InstalledMods, fetchedMods))
                return;

            InstalledMods.Clear();

            foreach (var mod in fetchedMods)
            {
                _logger.Debug("Found data for mod {ModName}", mod.ModName);
                InstalledMods.Add(mod);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load installed mods in ModsViewModel.");
        }
    }

    private bool AreSame(
        ObservableCollection<ModInstallationRecord> current,
        IEnumerable<ModInstallationRecord> incoming)
    {
        var incomingList = incoming as IList<ModInstallationRecord> ?? incoming.ToList();
        if (current.Count != incomingList.Count)
            return false;

        // Compare items individually by ModName
        return !current
            .Where((t, i) => !string.Equals(t.ModName, incomingList[i].ModName, StringComparison.Ordinal))
            .Any();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}