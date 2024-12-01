using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using PenumbraModForwarder.UI.Models;
using MenuItem = PenumbraModForwarder.UI.Models.MenuItem;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    private bool _isNotificationVisible;
    private string _notificationText;
    private double _progress;
    private MenuItem _selectedMenuItem;
    
    public ObservableCollection<MenuItem> MenuItems { get; }
    
    public MenuItem SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            if (value != null)
                CurrentPage = value.ViewModel;
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public bool IsNotificationVisible
    {
        get => _isNotificationVisible;
        set => this.RaiseAndSetIfChanged(ref _isNotificationVisible, value);
    }

    public string NotificationText
    {
        get => _notificationText;
        set => this.RaiseAndSetIfChanged(ref _notificationText, value);
    }

    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public ICommand MinimizeWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }
    
    public ICommand NavigateToSettingsCommand { get; }

    public MainWindowViewModel()
    {
        var app = Application.Current;
        MenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem("Home", 
                app?.Resources["HomeIcon"] as StreamGeometry ?? StreamGeometry.Parse(""), 
                new HomeViewModel()),
            new MenuItem("Mods", 
                app?.Resources["MenuIcon"] as StreamGeometry ?? StreamGeometry.Parse(""), 
                new ModsViewModel())
        };
        
        NavigateToSettingsCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new SettingsViewModel();
        });

        MinimizeWindowCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow!.WindowState = WindowState.Minimized;
            }
        });

        CloseWindowCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow?.Close();
            }
        });

        SelectedMenuItem = MenuItems[0];
        CurrentPage = SelectedMenuItem.ViewModel;

        ShowStartupNotification();
    }

    private async void ShowStartupNotification()
    {
        NotificationText = "Welcome to Penumbra Mod Forwarder";
        Progress = 0;
        IsNotificationVisible = true;

        for (int i = 0; i <= 100; i += 2)
        {
            Progress = i;
            await Task.Delay(20);
        }

        await Task.Delay(1000);
        IsNotificationVisible = false;
    }
}