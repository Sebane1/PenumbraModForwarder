using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using ReactiveUI;

public class FileSelectViewModel : ReactiveObject
{
    private readonly ILogger<FileSelectViewModel> _logger;

    public ObservableCollection<string> Files { get; } = new();

    private string[] _selectedFiles = Array.Empty<string>();
    public string[] SelectedFiles
    {
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }

    public ReactiveCommand<Unit, Unit> ConfirmSelectionCommand { get; }

    public FileSelectViewModel(ILogger<FileSelectViewModel> logger)
    {
        _logger = logger;

        ConfirmSelectionCommand = ReactiveCommand.Create(
            ConfirmSelection, 
            outputScheduler: RxApp.MainThreadScheduler // Ensure command runs on the UI thread
        );
    }

    public void LoadFiles(IEnumerable<string> files)
    {
        Files.Clear();
        foreach (var file in files)
        {
            Files.Add(file);
        }
    }

    private void ConfirmSelection()
    {
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            if (SelectedFiles.Any())
            {
                _logger.LogInformation($"Confirming selection of {SelectedFiles.Length} files.");
                _logger.LogInformation($"Selected files: {string.Join(", ", SelectedFiles)}");
            }
            else
            {
                _logger.LogWarning("No files selected.");
            }
        });
    }

}