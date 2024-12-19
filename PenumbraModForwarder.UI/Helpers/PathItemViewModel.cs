using System.Windows.Input;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Helpers;

public class PathItemViewModel : ReactiveObject
{
    private readonly ConfigurationPropertyDescriptor _parent;

    public string Path { get; }

    public ICommand RemoveCommand { get; }

    public PathItemViewModel(string path, ConfigurationPropertyDescriptor parent)
    {
        Path = path;
        _parent = parent;
        RemoveCommand = ReactiveCommand.Create(() => _parent.RemovePath(this));
    }
}