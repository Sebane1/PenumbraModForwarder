using System.Reactive;
using PenumbraModForwarder.UI.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Models;

public class Notification : ReactiveObject
{
    private bool _isVisible;
    private int _progress;
    private string _progressText;
    private bool _showProgress;
    private readonly INotificationService _notificationService;

    public string Text { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public Notification(string text, INotificationService notificationService, bool showProgress = true)
    {
        Text = text;
        _notificationService = notificationService;
        _showProgress = showProgress;
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public bool ShowProgress
    {
        get => _showProgress;
        set => this.RaiseAndSetIfChanged(ref _showProgress, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    private void Close()
    {
        _ = _notificationService.RemoveNotification(this);
    }
}