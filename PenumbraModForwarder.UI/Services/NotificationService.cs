using System;
using System.Collections.Generic;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;

namespace PenumbraModForwarder.UI.Services;

public class NotificationService : ReactiveObject, INotificationService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Notification> _progressNotifications = new();
    private const int FadeOutDuration = 500;
    private const int UpdateInterval = 100;

    public ObservableCollection<Notification> Notifications { get; } = new();

    public async Task ShowNotification(string message, int durationSeconds = 4)
    {
        var notification = new Notification(message, this, showProgress: true);
        
        lock (_lock)
        {
            if (Notifications.Count >= 3)
            {
                var oldestNotification = Notifications[0];
                oldestNotification.IsVisible = false;
                Task.Delay(FadeOutDuration).ContinueWith(_ =>
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

            notification.IsVisible = true;
            notification.Progress = 0;
            Notifications.Add(notification);
        }

        var elapsed = 0;
        var totalMs = durationSeconds * 1000;
        
        while (elapsed < totalMs && notification.IsVisible)
        {
            await Task.Delay(UpdateInterval);
            elapsed += UpdateInterval;
            notification.Progress = (int)((elapsed / (float)totalMs) * 100);
        }

        await RemoveNotification(notification);
    }

    public void UpdateProgress(string title, string status, int progress)
    {
        Log.Debug($"Updating progress for {title} to {status} : Progress: {progress}");
        lock (_lock)
        {
            if (!_progressNotifications.ContainsKey(title))
            {
                if (Notifications.Count >= 3)
                {
                    var oldestNotification = Notifications[0];
                    oldestNotification.IsVisible = false;
                    Task.Delay(FadeOutDuration).ContinueWith(_ =>
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

                var notification = new Notification(title, this, showProgress: true)
                {
                    IsVisible = true
                };
                _progressNotifications[title] = notification;
                Notifications.Add(notification);
            }

            var currentNotification = _progressNotifications[title];
            currentNotification.Progress = progress;
            currentNotification.ProgressText = status;

            if (progress >= 100)
            {
                currentNotification.IsVisible = false;
                Task.Delay(FadeOutDuration).ContinueWith(_ =>
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

    public async Task RemoveNotification(Notification notification)
    {
        notification.IsVisible = false;
        await Task.Delay(FadeOutDuration);
        
        lock (_lock)
        {
            Notifications.Remove(notification);
        }
    }
}