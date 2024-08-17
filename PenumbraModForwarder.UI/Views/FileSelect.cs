using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;


namespace PenumbraModForwarder.UI.Views
{
    public partial class FileSelect : Form, IViewFor<FileSelectViewModel>
    {
        public FileSelectViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FileSelectViewModel) value;
        }

        public FileSelect(FileSelectViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;

            this.WhenActivated(disposables =>
            {
                // Bind Files to CheckedListBox
                this.BindListBox(ViewModel, vm => vm.Files, fileCheckedListBox);

                // Bind the Confirm button command
                this.BindCommand(ViewModel, vm => vm.ConfirmSelectionCommand, v => v.confirmButton)
                    .DisposeWith(disposables);

                // Handle CheckedListBox selections
                fileCheckedListBox.ItemCheck += FileCheckedListBox_ItemCheck;

                // Add event handler disposal
                Disposable.Create(() => fileCheckedListBox.ItemCheck -= FileCheckedListBox_ItemCheck)
                    .DisposeWith(disposables);
            });
        }

        private void FileCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                var checkedItems = fileCheckedListBox.Items.Cast<string>()
                    .Where((item, index) => fileCheckedListBox.GetItemChecked(index))
                    .ToArray();
                ViewModel.SelectedFiles = checkedItems;
            }));
        }

        private void BindListBox(FileSelectViewModel viewModel,
            System.Linq.Expressions.Expression<Func<FileSelectViewModel, ObservableCollection<string>>> vmProperty,
            CheckedListBox listBox)
        {
            viewModel.WhenAnyValue(vmProperty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(files =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        listBox.Items.Clear();
                        foreach (var file in files)
                        {
                            listBox.Items.Add(file, false);
                        }
                    }));
                });
        }
    }
}
