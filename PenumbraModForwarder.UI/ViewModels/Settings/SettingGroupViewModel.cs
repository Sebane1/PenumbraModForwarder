using System.Collections.ObjectModel;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels.Settings;

public class SettingGroupViewModel : ReactiveObject
{
    public string GroupName { get; set; }

    public ObservableCollection<SettingViewModel> Settings { get; set; } = new();
}