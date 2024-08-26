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
    private string _archiveFileName = string.Empty;
    private bool _showAllSelectedVisible;
    private bool _showAllSelectedEnabled;

    public bool ShowAllSelectedVisible
    {
        get => _showAllSelectedVisible;
        set => this.RaiseAndSetIfChanged(ref _showAllSelectedVisible, value);
    }
    
    public bool ShowAllSelectedEnabled
    {
        get => _showAllSelectedEnabled;
        set => this.RaiseAndSetIfChanged(ref _showAllSelectedEnabled, value);
    }

    public string ArchiveFileName
    {
        get => _archiveFileName;
        set => this.RaiseAndSetIfChanged(ref _archiveFileName, value);
    }
    
    public string[] SelectedFiles
    {
        get => _selectedFiles;
        set => this.RaiseAndSetIfChanged(ref _selectedFiles, value);
    }

    public ReactiveCommand<Unit, Unit> ConfirmSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelSelectionCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }
    public Action CloseAction { get; set; }

    public FileSelectViewModel(ILogger<FileSelectViewModel> logger)
    {
        _logger = logger;
        ConfirmSelectionCommand = ReactiveCommand.Create(
            ConfirmSelection,
            outputScheduler: RxApp.MainThreadScheduler
        );
        
        CancelSelectionCommand = ReactiveCommand.Create(
            CancelSelection,
            outputScheduler: RxApp.MainThreadScheduler
        );
        
        SelectAllCommand = ReactiveCommand.Create(
            SelectAll,
            outputScheduler: RxApp.MainThreadScheduler
        );
    }

    public void LoadFiles(IEnumerable<string> files)
    {
        Files.Clear();
        
        var fileNameCounts = new Dictionary<string, int>();
        var enumerable = files as string[] ?? files.ToArray();
        
        foreach (var file in enumerable)
        {
            var fileName = Path.GetFileName(file);
            
            if (fileNameCounts.TryGetValue(fileName, out var value))
            {
                fileNameCounts[fileName] = ++value;
            }
            else
            {
                fileNameCounts[fileName] = 1;
            }
        }

        foreach (var file in enumerable)
        {
            var fileName = Path.GetFileName(file);
            
            var displayName = fileNameCounts[fileName] > 1 ? $"{fileName} ({file})" : fileName;

            var fileItem = new FileItem
            {
                FullPath = file,
                FileName = displayName
            };

            Files.Add(fileItem);
        }

        // Show and enable the Select All button if there are more than 3 files
        ShowAllSelectedVisible = Files.Count > 3;
        ShowAllSelectedEnabled = Files.Count > 3;
    }

    private void SelectAll()
    {
        SelectedFiles = Files.Count == SelectedFiles.Length ? [] : Files.Select(file => file.FullPath).ToArray();
    }


    private void CancelSelection()
    {
        _logger.LogWarning("File selection was canceled.");
        // We should clear the selected files array here too, just in case
        SelectedFiles = [];
        CloseAction?.Invoke();
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
