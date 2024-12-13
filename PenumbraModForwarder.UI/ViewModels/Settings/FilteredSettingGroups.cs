using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class FilteredSettingGroups : ReactiveObject
{
    public ObservableCollection<SettingGroupViewModel> FilteredGroups { get; }

    public FilteredSettingGroups(ObservableCollection<SettingGroupViewModel> settingGroups, string filter)
    {
        FilteredGroups = new ObservableCollection<SettingGroupViewModel>(
            settingGroups.Where(g => g.GroupName == filter)
        );
    }
}