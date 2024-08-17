using System.Reactive.Disposables;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views;

public partial class MainWindow : Form, IViewFor<MainWindowViewModel>
{
    public MainWindowViewModel ViewModel { get; set; }
    
    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel) value;
    }
    
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.SelectedFolderPath, v => v.directory_text.Text)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenFolderDialog, v => v.select_directory)
                .DisposeWith(disposables);
            
            
            this.Bind(ViewModel, vm => vm.AutoDelete, v => v.autodelete_checkbox.Checked)
                .DisposeWith(disposables);
            
            autodelete_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateAutoDeleteCommand.Execute(autodelete_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.AutoLoad, v => v.autoforward_checkbox.Checked)
                .DisposeWith(disposables);
            
            autoforward_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateAutoLoadCommand.Execute(autoforward_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.ExtractAll, v => v.extractall_checkbox.Checked)
                .DisposeWith(disposables);

            extractall_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateExtractAllCommand.Execute(extractall_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.SelectBoxEnabled, v => v.select_directory.Enabled)
                .DisposeWith(disposables);
        });
    }
}