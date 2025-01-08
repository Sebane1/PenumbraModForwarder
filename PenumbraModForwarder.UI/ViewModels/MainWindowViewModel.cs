using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Extensions;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Models;
using PenumbraModForwarder.UI.Services;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
    private readonly IWebSocketClient _webSocketClient;
    private readonly ISoundManagerService _soundManagerService;
    private readonly IConfigurationListener _configurationListener;
    private readonly IConfigurationService _configurationService;

    private ViewModelBase _currentPage = null!;
    private MenuItem _selectedMenuItem = null!;

    public ObservableCollection<MenuItem> MenuItems { get; }
    public ObservableCollection<Notification> Notifications =>
        (_notificationService as NotificationService)?.Notifications ?? new();

    public InstallViewModel InstallViewModel { get; }

    public MenuItem SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            if (value != null)
            {
                CurrentPage = value.ViewModel;
            }
        }
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public ICommand NavigateToSettingsCommand { get; }

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        INotificationService notificationService,
        IWebSocketClient webSocketClient,
        int port,
        IConfigurationListener configurationListener,
        ISoundManagerService soundManagerService, IConfigurationService configurationService)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _webSocketClient = webSocketClient;
        _configurationListener = configurationListener;
        _soundManagerService = soundManagerService;
        _configurationService = configurationService;

        if ((bool)_configurationService.ReturnConfigValue(c => c.Common.EnableSentry))
        {
            _logger.Info("Enabling Sentry");
            DependencyInjection.EnableSentryLogging();
        }

        var app = Application.Current;

        var homeViewModel = _serviceProvider.GetRequiredService<HomeViewModel>();
        var modsViewModel = _serviceProvider.GetRequiredService<ModsViewModel>();

        MenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem(
                "Home",
                app?.Resources["HomeIcon"] as StreamGeometry ?? StreamGeometry.Parse(""),
                homeViewModel
            ),
            new MenuItem(
                "Mods",
                app?.Resources["MenuIcon"] as StreamGeometry ?? StreamGeometry.Parse(""),
                modsViewModel
            )
        };

        NavigateToSettingsCommand = ReactiveCommand.Create(() =>
        {
            SelectedMenuItem = null;
            CurrentPage = ActivatorUtilities.CreateInstance<SettingsViewModel>(_serviceProvider);
        });

        _selectedMenuItem = MenuItems[0];
        _currentPage = _selectedMenuItem.ViewModel;

        InstallViewModel = new InstallViewModel(_webSocketClient, _soundManagerService);

        _ = InitializeWebSocketConnection(port);
    }

    private async Task InitializeWebSocketConnection(int port)
    {
        try
        {
            await Task.Run(() => _webSocketClient.ConnectAsync(port));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize WebSocket connection");
            await _notificationService.ShowNotification("Failed to connect to background service");
        }
    }
}