using System.ComponentModel;
using System.Reactive.Disposables;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace PenumbraModForwarder.UI.Views;

public partial class ErrorWindow : Form, IViewFor<ErrorWindowViewModel>
{
    [DefaultValue(null)] public ErrorWindowViewModel ViewModel { get; set; }
    
    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (ErrorWindowViewModel)value;
    }
    
    public ErrorWindow(ErrorWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.ErrorMessage, v => v.error_TextBox.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.OpenLogFolderCommand, v => v.openlog_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenDiscordCommand, v => v.openDiscord_Button)
                .DisposeWith(disposables);
        });
    }
}