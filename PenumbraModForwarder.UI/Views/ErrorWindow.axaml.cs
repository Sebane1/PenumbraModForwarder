using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Serilog;

namespace PenumbraModForwarder.UI.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
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