using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services;

public class ErrorWindowService : IErrorWindowService
{
    private readonly ILogger<ErrorWindowViewModel> _errorWindowLogger;
    private readonly ILogger<ErrorWindowService> _logger;
    private readonly IProcessHelperService _processHelperService;

    public ErrorWindowService(ILogger<ErrorWindowViewModel> errorWindowLogger, ILogger<ErrorWindowService> logger, IProcessHelperService processHelperService)
    {
        _errorWindowLogger = errorWindowLogger;
        _logger = logger;
        _processHelperService = processHelperService;
    }
    
    public void ShowError(string message)
    {
        if (Application.OpenForms.Count > 0)
        {
            // Ensure we use the UI thread for showing the form
            Application.OpenForms[0].Invoke(() => ShowErrorInternal(message));
        }
        else
        {
            ShowErrorInternal(message);
        }
    }
    
    private void ShowErrorInternal(string message)
    {
        // We need to manually pass in _processHelperService here, as the constructor of ErrorWindowViewModel won't have access to it.
        using var errorWindow = new ErrorWindow(new ErrorWindowViewModel(_errorWindowLogger, _processHelperService));
        errorWindow.ViewModel.ErrorMessage = message;
        errorWindow.ShowDialog();
    }
}