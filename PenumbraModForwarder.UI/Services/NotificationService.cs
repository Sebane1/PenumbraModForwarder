using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Services;
public class NotificationService : ReactiveObject, INotificationService
{
    private Notification _currentProgressNotification;
    public ObservableCollection<Notification> Notifications { get; } = new();

    public async Task ShowNotification(string message)
    {
        if (Notifications.Count >= 3)
        {
            var oldestNotification = Notifications[0];
            Notifications.RemoveAt(0);
        }

        var notification = new Notification(message)
        {
            Progress = -1 // Don't show progress bar
        };
        Notifications.Add(notification);

        await Task.Delay(3000);
        notification.IsVisible = false;
        await Task.Delay(300);
        Notifications.Remove(notification);
    }

    public void UpdateProgress(string title, string status, int progress)
    {
        if (_currentProgressNotification == null)
        {
            _currentProgressNotification = new Notification(title);
            Notifications.Add(_currentProgressNotification);
        }

        _currentProgressNotification.Progress = progress;
        _currentProgressNotification.ProgressText = status;

        if (progress >= 100)
        {
            _currentProgressNotification.IsVisible = false;
            Notifications.Remove(_currentProgressNotification);
            _currentProgressNotification = null;
        }
    }
}