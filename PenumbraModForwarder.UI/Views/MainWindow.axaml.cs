using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PenumbraModForwarder.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var titleBar = this.FindControl<Grid>("TitleBar");
        titleBar?.AddHandler(PointerPressedEvent, (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }, RoutingStrategies.Tunnel);
    }
}