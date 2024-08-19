using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

public class FileSelectViewModel : ReactiveObject
{
    private readonly ILogger<FileSelectViewModel> _logger;

    public ObservableCollection<FileItem> Files { get; } = new();

    private string[] _selectedFiles = Array.Empty<string>();
    public string[] SelectedFiles
    {
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }

    public ReactiveCommand<Unit, Unit> ConfirmSelectionCommand { get; }
    public Action CloseAction { get; set; }

    public FileSelectViewModel(ILogger<FileSelectViewModel> logger)
    {
        _logger = logger;
        ConfirmSelectionCommand = ReactiveCommand.Create(
            ConfirmSelection,
            outputScheduler: RxApp.MainThreadScheduler
        );
    }

    public void LoadFiles(IEnumerable<string> files)
    {
        Files.Clear();
        foreach (var file in files)
        {
            var fileItem = new FileItem
            {
                FullPath = file,
                FileName = Path.GetFileName(file)
            };
            Files.Add(fileItem);
        }
    }


    private void ConfirmSelection()
    {
        // This should already be on the UI thread due to the scheduler
        if (SelectedFiles.Any())
        {
            _logger.LogInformation($"Confirming selection of {SelectedFiles.Length} files.");
            _logger.LogInformation($"Selected files: {string.Join(", ", SelectedFiles)}");
            
            CloseAction?.Invoke();
        }
        else
        {
            _logger.LogWarning("No files selected.");
        }
    }
}