using ReactiveUI;

namespace PenumbraModForwarder.UI.Models;

public class Notification : ReactiveObject
{
    private double _progress;
    private bool _isVisible = true;
    private string _progressText = string.Empty;

    public string Text { get; }
    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }
    
    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    public Notification(string text)
    {
        Text = text;
    }
}