using System.Collections.Generic;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Services;

public class NotificationService : ReactiveObject, INotificationService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Notification> _progressNotifications = new();
    public ObservableCollection<Notification> Notifications { get; } = new();

    public async Task ShowNotification(string message)
    {
        Notification notification;
        lock (_lock)
        {
            if (Notifications.Count >= 3)
            {
                var oldestNotification = Notifications[0];
                oldestNotification.IsVisible = false;
                Task.Delay(500).ContinueWith(_ =>
                {
                    lock (_lock)
                    {
                        if (Notifications.Count > 0)
                        {
                            Notifications.RemoveAt(0);
                        }
                    }
                });
            }

            notification = new Notification(message)
            {
                IsVisible = true,
                Progress = -1 // Don't show progress bar
            };
            Notifications.Add(notification);
        }

        await Task.Delay(4000);
        notification.IsVisible = false;
        await Task.Delay(500);

        lock (_lock)
        {
            Notifications.Remove(notification);
        }
    }

    public void UpdateProgress(string title, string status, int progress)
    {
        lock (_lock)
        {
            if (!_progressNotifications.ContainsKey(title))
            {
                if (Notifications.Count >= 3)
                {
                    var oldestNotification = Notifications[0];
                    oldestNotification.IsVisible = false;
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        lock (_lock)
                        {
                            if (Notifications.Count > 0)
                            {
                                Notifications.RemoveAt(0);
                            }
                        }
                    });
                }

                var notification = new Notification(title)
                {
                    IsVisible = true
                };
                _progressNotifications[title] = notification;
                Notifications.Add(notification);
            }
        }

        var currentNotification = _progressNotifications[title];
        currentNotification.Progress = progress;
        currentNotification.ProgressText = status;

        if (progress >= 100)
        {
            currentNotification.IsVisible = false;
            Task.Delay(500).ContinueWith(_ =>
            {
                lock (_lock)
                {
                    Notifications.Remove(currentNotification);
                    _progressNotifications.Remove(title);
                }
            });
        }
    }
}