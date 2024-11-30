using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Windows.Input;
using Serilog;

namespace PenumbraModForwarder.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public ICommand MinimizeWindowCommand { get; }
    public ICommand CloseWindowCommand { get; }

    public MainWindowViewModel()
    {
        // These are currently broken
        MinimizeWindowCommand = ReactiveCommand.Create<Window>(window =>
        {
            Log.Debug("Minimize command executed");
            window.WindowState = WindowState.Minimized;
        });

        CloseWindowCommand = ReactiveCommand.Create<Window>(window =>
        {
            Log.Debug("Close command executed");
            window.Close();
        });

        CurrentPage = new HomeViewModel();
    }
}