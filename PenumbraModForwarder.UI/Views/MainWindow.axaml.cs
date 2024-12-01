using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Serilog;

namespace PenumbraModForwarder.UI.Views;

public partial class MainWindow : Window
{
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
            Log.Information("Minimize button clicked");
            WindowState = WindowState.Minimized;
        };

        this.Get<Button>("CloseButton").Click += (s, e) =>
        {
            Log.Information("Close button clicked");
            Close();
        };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        Log.Information("Window closing");
        Environment.Exit(0);
    }
}