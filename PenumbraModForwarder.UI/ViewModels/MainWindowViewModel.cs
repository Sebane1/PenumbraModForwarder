using System;
using Avalonia;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.UI.Models;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PenumbraModForwarder.UI.Services;
using Serilog;
using MenuItem = PenumbraModForwarder.UI.Models.MenuItem;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
    private readonly IWebSocketClient _webSocketClient;
    private ViewModelBase _currentPage = null!;
    private MenuItem _selectedMenuItem = null!;

    public ObservableCollection<MenuItem> MenuItems { get; }
    public ObservableCollection<Notification> Notifications => 
        (_notificationService as NotificationService)?.Notifications ?? new();

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

    public ICommand NavigateToSettingsCommand { get; }

    public MainWindowViewModel(IServiceProvider serviceProvider, INotificationService notificationService, IWebSocketClient webSocketClient)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _webSocketClient = webSocketClient;
        var app = Application.Current;

        MenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem("Home",
                app?.Resources["HomeIcon"] as StreamGeometry ?? StreamGeometry.Parse(""),
                ActivatorUtilities.CreateInstance<HomeViewModel>(_serviceProvider)),
            new MenuItem("Mods",
                app?.Resources["MenuIcon"] as StreamGeometry ?? StreamGeometry.Parse(""),
                ActivatorUtilities.CreateInstance<ModsViewModel>(_serviceProvider))
        };

        NavigateToSettingsCommand = ReactiveCommand.Create(() =>
        {
            SelectedMenuItem = null;
            CurrentPage = ActivatorUtilities.CreateInstance<SettingsViewModel>(_serviceProvider);
        });

        _selectedMenuItem = MenuItems[0];
        _currentPage = _selectedMenuItem.ViewModel;
        
        _ = InitializeWebSocketConnection();
    }

    private async Task InitializeWebSocketConnection()
    {
        try
        {
            await Task.Run(() => _webSocketClient.ConnectAsync());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize WebSocket connection");
            await _notificationService.ShowNotification("Failed to connect to background service");
        }
    }
}