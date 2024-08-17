using Microsoft.Extensions.Logging;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services;

public class UserInteractionService : IUserInteractionService
{
    // We need to inject the logger for the FileSelectViewModel
    private readonly ILogger<FileSelectViewModel> _fileSelectLogger;
    
    private readonly ILogger<UserInteractionService> _logger;

    public UserInteractionService(ILogger<FileSelectViewModel> fileSelectLogger, ILogger<UserInteractionService> logger)
    {
        _fileSelectLogger = fileSelectLogger;
        _logger = logger;
    }

    public string ShowFileSelectionDialog(string[] files)
    {
        _logger.LogInformation("Showing file selection dialog.");
        var fileSelectViewModel = new FileSelectViewModel(_fileSelectLogger);
        fileSelectViewModel.LoadFiles(files);
        
        using var fileSelect = new FileSelect(fileSelectViewModel);
        return fileSelect.ShowDialog() == DialogResult.OK ? fileSelectViewModel.SelectedFile : null;
    }
}