using System.Collections.ObjectModel;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private ObservableCollection<InfoItem> _infoItems;

    public ObservableCollection<InfoItem> InfoItems
    {
        get => _infoItems;
        set => this.RaiseAndSetIfChanged(ref _infoItems, value);
    }
    
    public HomeViewModel()
    {
        // Initialize with some sample data
        InfoItems =
        [
            new("Item 1", 100),
            new("Item 2", 200),
            new("Item 3", 300)
        ];
    }
}