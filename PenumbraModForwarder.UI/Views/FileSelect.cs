using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace PenumbraModForwarder.UI.Views
{
    public partial class FileSelect : Form, IViewFor<FileSelectViewModel>
    {
        public FileSelectViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FileSelectViewModel)value;
        }

        public FileSelect(FileSelectViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            
            this.WhenActivated(disposables =>
            {
                fileListBox.DataSource = ViewModel.Files;
                
                fileListBox.SelectedIndexChanged += (sender, args) =>
                {
                    if (fileListBox.SelectedItem != null)
                    {
                        ViewModel.SelectedFile = fileListBox.SelectedItem.ToString();
                    }
                };

                // This is optional: if you want to automatically select the first item
                this.ViewModel.WhenAnyValue(vm => vm.Files.Count)
                    .Where(count => count > 0)
                    .Subscribe(_ => fileListBox.SelectedIndex = 0)
                    .DisposeWith(disposables);
            });
        }
    }
}