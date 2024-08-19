using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

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
            ViewModel.CloseAction = () =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            this.WhenActivated(disposables =>
            {
                BindListBox(ViewModel, vm => vm.Files, fileCheckedListBox);

                this.BindCommand(ViewModel, vm => vm.ConfirmSelectionCommand, v => v.confirmButton)
                    .DisposeWith(disposables);

                fileCheckedListBox.ItemCheck += FileCheckedListBox_ItemCheck;

                // Dispose event handler
                Disposable.Create(() => fileCheckedListBox.ItemCheck -= FileCheckedListBox_ItemCheck)
                    .DisposeWith(disposables);
            });
        }

        private void FileCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(() =>
            {
                var checkedItems = fileCheckedListBox.Items.Cast<FileItem>()
                    .Where((item, index) => 
                        (index == e.Index ? e.NewValue == CheckState.Checked : fileCheckedListBox.GetItemChecked(index)))
                    .Select(item => item.FullPath)
                    .ToArray();

                ViewModel.SelectedFiles = checkedItems;
            });
        }

        private void BindListBox(FileSelectViewModel viewModel,
            System.Linq.Expressions.Expression<Func<FileSelectViewModel, ObservableCollection<FileItem>>> vmProperty,
            CheckedListBox listBox)
        {
            viewModel.WhenAnyValue(vmProperty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(files =>
                {
                    BeginInvoke(() =>
                    {
                        listBox.Items.Clear();
                        foreach (var file in files)
                        {
                            listBox.Items.Add(file, false);
                        }
                    });
                });
        }
    }
}
