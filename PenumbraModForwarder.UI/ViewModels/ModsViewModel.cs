using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using ReactiveUI;
using Serilog;

namespace PenumbraModForwarder.UI.ViewModels;

public class ModsViewModel : ViewModelBase
{
    private readonly IStatisticService _statisticService;
    private readonly ILogger _logger;
    private readonly CompositeDisposable _disposables = new();

    private ObservableCollection<ModInstallationRecord> _installedMods;

    public ObservableCollection<ModInstallationRecord> InstalledMods
    {
        get => _installedMods;
        set => this.RaiseAndSetIfChanged(ref _installedMods, value);
    }
    
    public ModsViewModel(IStatisticService statisticService)
    {
        _logger = Log.ForContext<ModsViewModel>();
        _statisticService = statisticService;
        
        InstalledMods = new ObservableCollection<ModInstallationRecord>();
        
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

            // If the list is exactly the same (including count, names, etc.), skip updates
            if (AreSame(InstalledMods, fetchedMods))
            {
                return;
            }

            InstalledMods.Clear();
            foreach (var mod in fetchedMods)
            {
                _logger.Debug($"Found data for mod {mod.ModName}");
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

        return !current.Where((t, i) => !string.Equals(t.ModName, incomingList[i].ModName, StringComparison.Ordinal)).Any();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}