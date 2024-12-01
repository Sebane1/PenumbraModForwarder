using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Services;
public class NotificationService : ReactiveObject, INotificationService
{
    private Dictionary<string, Notification> _progressNotifications = new();
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
            Progress = -1
        };
        Notifications.Add(notification);

        await Task.Delay(3000);
        notification.IsVisible = false;
        await Task.Delay(300);
        Notifications.Remove(notification);
    }

    public void UpdateProgress(string title, string status, int progress)
    {
        if (!_progressNotifications.ContainsKey(title))
        {
            if (Notifications.Count >= 3)
            {
                var oldestNotification = Notifications[0];
                Notifications.RemoveAt(0);
            }

            var notification = new Notification(title);
            _progressNotifications[title] = notification;
            Notifications.Add(notification);
        }

        var currentNotification = _progressNotifications[title];
        currentNotification.Progress = progress;
        currentNotification.ProgressText = status;

        if (progress >= 100)
        {
            currentNotification.IsVisible = false;
            Notifications.Remove(currentNotification);
            _progressNotifications.Remove(title);
        }
    }
}