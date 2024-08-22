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
    }

    
    private void CancelSelection()
    {
        _logger.LogWarning("File selection was canceled.");
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