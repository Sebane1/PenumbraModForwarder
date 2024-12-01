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
using MenuItem = PenumbraModForwarder.UI.Models.MenuItem;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
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

    public MainWindowViewModel(IServiceProvider serviceProvider, INotificationService notificationService)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
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

        _ = ShowWelcomeSequence();
        _ = ShowMultipleProgressExample();
    }

    private async Task ShowWelcomeSequence()
    {
        _notificationService.UpdateProgress(
            "Welcome",
            "Loading application...",
            0);

        for (int i = 0; i <= 100; i += 10)
        {
            _notificationService.UpdateProgress(
                "Welcome",
                $"Loading application... {i}%",
                i);
            await Task.Delay(200);
        }

        await _notificationService.ShowNotification("Welcome to Penumbra Mod Forwarder!");
    }
    
    private async Task ShowMultipleProgressExample()
    {
        _notificationService.UpdateProgress("Task 1", "Starting first task...", 0);
        _notificationService.UpdateProgress("Task 2", "Starting second task...", 0);

        for (int i = 0; i <= 100; i += 30)
        {
            _notificationService.UpdateProgress("Task 1", $"Processing task 1... {i}%", i);
            _notificationService.UpdateProgress("Task 2", $"Processing task 2... {i}%", i);
            await Task.Delay(200);
        }

        await _notificationService.ShowNotification("All tasks completed!");
    }
}