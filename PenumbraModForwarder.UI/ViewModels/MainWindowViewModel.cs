using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MenuItem = PenumbraModForwarder.UI.Models.MenuItem;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private ViewModelBase _currentPage = null!;
    private bool _isNotificationVisible;
    private string _notificationText = string.Empty;
    private double _progress;
    private MenuItem _selectedMenuItem = null!;

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

    public ICommand NavigateToSettingsCommand { get; }

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
            CurrentPage = ActivatorUtilities.CreateInstance<SettingsViewModel>(_serviceProvider);
        });

        _selectedMenuItem = MenuItems[0];
        _currentPage = _selectedMenuItem.ViewModel;
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