using System.Collections.ObjectModel;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class SettingGroupViewModel : ViewModelBase
{
    public string GroupName { get; set; }
    public ObservableCollection<SettingViewModel> Settings { get; set; } = new();
}