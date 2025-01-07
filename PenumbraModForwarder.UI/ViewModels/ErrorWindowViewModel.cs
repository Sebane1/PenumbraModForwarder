namespace PenumbraModForwarder.UI.ViewModels;

public class ErrorWindowViewModel : ViewModelBase
{
    public string ErrorMessage { get; } = "Please launch PenumbraModForwarder.Launcher.exe.\n" +
                                          "This ensures proper monitoring and crash recovery.";
}