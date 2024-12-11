using System.Collections.ObjectModel;
using System.Linq;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class FilteredSettingGroups : ViewModelBase
{
    public ObservableCollection<SettingGroupViewModel> FilteredGroups { get; }

    public FilteredSettingGroups(ObservableCollection<SettingGroupViewModel> settingGroups, string filter)
    {
        FilteredGroups = new ObservableCollection<SettingGroupViewModel>(
            settingGroups.Where(g => g.Settings.Any(s => s.GroupName == filter)));
    }
}