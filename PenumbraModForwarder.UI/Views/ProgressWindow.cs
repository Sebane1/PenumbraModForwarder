using System.Reactive.Disposables;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views
{
    public partial class ProgressWindow : Form, IViewFor<ProgressWindowViewModel>
    {
        public ProgressWindowViewModel ViewModel { get; set; }
        
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ProgressWindowViewModel)value;
        }
        
        public ProgressWindow(ProgressWindowViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.FileName, v => v.file_Label.Text)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.Progress, v => v.progress_Bar.Value)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.Operation, v => v.operation_Label.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
