using System.Diagnostics;
using System.Reactive;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class ErrorWindowViewModel : ReactiveObject
{
    private readonly ILogger<ErrorWindowViewModel> _logger;
    private readonly IProcessHelperService _processHelperService;
    
    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }
    
    public ReactiveCommand<Unit, Unit> OpenLogFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenDiscordCommand { get; }

    public ErrorWindowViewModel(ILogger<ErrorWindowViewModel> logger, IProcessHelperService processHelperService)
    {
        _logger = logger;
        _processHelperService = processHelperService;
        OpenLogFolderCommand = ReactiveCommand.Create(OpenLogFolder);
        OpenDiscordCommand = ReactiveCommand.Create(OpenDiscord);
    }

    private void OpenLogFolder()
    {
        _processHelperService.OpenLogFolder();
    }

    private void OpenDiscord()
    {
        _processHelperService.OpenSupportDiscord();
    }
}