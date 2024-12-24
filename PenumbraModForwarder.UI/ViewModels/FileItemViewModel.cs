using ReactiveUI;

namespace PenumbraModForwarder.UI.ViewModels;

public class FileItemViewModel : ReactiveObject
{
    private string _fileName;
    private string _filePath;
    private bool _isSelected;

    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}