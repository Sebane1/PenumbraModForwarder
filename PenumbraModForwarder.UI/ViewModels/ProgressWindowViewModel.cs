using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class ProgressWindowViewModel : ReactiveObject
{
    private readonly ILogger<ProgressWindowViewModel> _logger;
    
    private string _fileName = string.Empty;
    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }
    
    private string _operation = string.Empty;
    public string Operation
    {
        get => _operation;
        set => this.RaiseAndSetIfChanged(ref _operation, value);
    }
    
    private int _progress;
    public int Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    public ProgressWindowViewModel(ILogger<ProgressWindowViewModel> logger)
    {
        _logger = logger;
    }
}