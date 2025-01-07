using Avalonia.Controls;
using Serilog;

namespace PenumbraModForwarder.Updater.Views;

public partial class MainWindow : Window
{
    private readonly ILogger _logger;

    public MainWindow()
    {
        InitializeComponent();

        _logger = Log.ForContext<MainWindow>();

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
            _logger.Information("Minimize button clicked");
            WindowState = WindowState.Minimized;
        };

        this.Get<Button>("CloseButton").Click += (s, e) =>
        {
            _logger.Information("Close button clicked");
            Close();
        };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _logger.Information("Window closing");
    }
}