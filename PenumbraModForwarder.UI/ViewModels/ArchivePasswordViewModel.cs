using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class ArchivePasswordViewModel : ReactiveObject
{
    private readonly ILogger<ArchivePasswordViewModel> _logger;

    private string _fileName = string.Empty;
    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }
    
    private string _password = string.Empty;
    
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }
    
    public Action CloseAction { get; set; }
    public ReactiveCommand<Unit, Unit> ConfirmInputCommand { get; }
    
    public ArchivePasswordViewModel(ILogger<ArchivePasswordViewModel> logger)
    {
        _logger = logger;
        ConfirmInputCommand = ReactiveCommand.Create(
            ConfirmInput,
            outputScheduler: RxApp.MainThreadScheduler
        );
    }
    
    private void ConfirmInput()
    {
        _logger.LogInformation("Confirming archive password input.");
        CloseAction?.Invoke();
    }
    
    private void CancelInput()
    {
        _logger.LogInformation("Canceling archive password input.");
        CloseAction?.Invoke();
    }
}