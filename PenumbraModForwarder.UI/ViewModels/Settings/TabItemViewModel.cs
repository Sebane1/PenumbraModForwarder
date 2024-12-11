using System.Collections.ObjectModel;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class TabItemViewModel
{
    public string Header { get; set; }
    public ObservableCollection<SettingGroupViewModel> SettingGroups { get; set; }
}