using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.ViewModels;

namespace PenumbraModForwarder.UI.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnViewModClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button {Tag: XmaMods mod})
        {
            var url = mod.ModUrl;
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
    
    private async void OnDownloadModClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel vm
            && sender is Button {Tag: XmaMods mod})
        {
            await vm.DownloadModsAsync(mod);
        }
    }
}