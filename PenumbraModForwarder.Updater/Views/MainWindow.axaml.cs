using Avalonia.Controls;
using NLog;

namespace PenumbraModForwarder.Updater.Views;

public partial class MainWindow : Window
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public MainWindow()
    {
        InitializeComponent();

        var titleBar = this.FindControl<Grid>("TitleBar");
        titleBar.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        };

        // Direct event handling for window controls
        this.Get<Button>("MinimizeButton").Click += (s, e) =>
        {
            _logger.Info("Minimize button clicked");
            WindowState = WindowState.Minimized;
        };

        this.Get<Button>("CloseButton").Click += (s, e) =>
        {
            _logger.Info("Close button clicked");
            Close();
        };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _logger.Info("Window closing");
    }
}