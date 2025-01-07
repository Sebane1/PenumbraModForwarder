using PenumbraModForwarder.Updater.Interfaces;

namespace PenumbraModForwarder.Updater.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IGetBackgroundInformation _getBackgroundInformation;
    public string Greeting { get; } = "Welcome to Avalonia!";

    public MainWindowViewModel(IGetBackgroundInformation getBackgroundInformation)
    {
        _getBackgroundInformation = getBackgroundInformation;
        Begin();
    }

    private void Begin()
    {
        _getBackgroundInformation.GetResources();
    }
}